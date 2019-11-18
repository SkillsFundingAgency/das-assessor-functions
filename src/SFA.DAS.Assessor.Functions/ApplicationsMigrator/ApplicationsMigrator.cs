using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.Assessor.Functions.Infrastructure;


namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public class ApplicationsMigrator
    {
        private readonly SqlConnectionStrings _connectionStrings;
        private readonly IQnaDataTranslator _qnaDataTranslator;
        private readonly IDataAccess _dataAccess;

        public ApplicationsMigrator(IOptions<SqlConnectionStrings> connectionStrings, IQnaDataTranslator qnaDataTranslator, IDataAccess dataAccess)
        {
            _connectionStrings = connectionStrings.Value;
            _qnaDataTranslator = qnaDataTranslator;
            this._dataAccess = dataAccess;
        }

        [FunctionName("ApplicationsMigrator")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation($"ApplicationsMigrator - HTTP trigger function executed at: {DateTime.Now}");

            using (var applyConnection = new SqlConnection(_connectionStrings.Apply))
            using (var qnaConnection = new SqlConnection(_connectionStrings.QnA))
            using (var assessorConnection = new SqlConnection(_connectionStrings.Assessor))
            {
                var workflowId = _dataAccess.GetEpaoWorkflowId(qnaConnection);
                if (workflowId is null)
                {
                    throw new ApplicationException("Workflow of Type 'EPAO' not found.");
                }

                var applyApplications = _dataAccess.GetCurrentApplyApplications(applyConnection);
                
                int totalApplicationsToMigrate = applyApplications.Count;
                log.LogInformation($"Number of applications to Migrate: {totalApplicationsToMigrate}");

                var applicationsMigrated = 0;
                foreach (var originalApplyApplication in applyApplications)
                {
                    Guid qnaApplicationId = _dataAccess.CreateQnaApplicationRecord(qnaConnection, workflowId, originalApplyApplication);

                    var applySequences = _dataAccess.GetCurrentApplyApplicationSequences(applyConnection, originalApplyApplication);
                    var applySections = _dataAccess.GetCurrentApplyApplicationSections(applyConnection, originalApplyApplication);

                    foreach (var applySequence in applySequences)
                    {
                        _dataAccess.CreateQnaApplicationSequencesRecord(qnaConnection, qnaApplicationId, applySequence);

                        foreach (var applySection in applySections)
                        {
                            if (applySection.SequenceId == applySequence.SequenceId)
                            {
                                applySection.QnAData = _qnaDataTranslator.Translate(applySection, log);

                                _dataAccess.CreateQnaApplicationSectionsRecord(qnaConnection, qnaApplicationId, applySequence, applySection);
                            }
                        }
                    }

                    var applyingOrganisation = _dataAccess.GetApplyingOrganisation(applyConnection, originalApplyApplication.ApplyingOrganisationId);

                    Guid? organisationId = null;
                    if (!applyingOrganisation.RoEPAOApproved)
                    {
                        organisationId = _dataAccess.CreateNewOrganisation(assessorConnection, originalApplyApplication);
                    }
                    else
                    {
                        organisationId = _dataAccess.GetExistingOrganisation(assessorConnection, applyingOrganisation);
                    }

                    if (organisationId != default(Guid))
                    {
                        dynamic applyDataObject = GenerateApplyData(originalApplyApplication, applySequences, applySections);
                        dynamic financialGradeObject = CreateFinancialGradeObject(originalApplyApplication);

                        _dataAccess.CreateAssessorApplyRecord(assessorConnection, originalApplyApplication, qnaApplicationId, organisationId, applyDataObject, financialGradeObject);

                        // Convert ApplicationData
                    }

                    applicationsMigrated++;

                    if (applicationsMigrated % 10 == 0 || applicationsMigrated == totalApplicationsToMigrate)
                    {
                        log.LogInformation($"Completed {applicationsMigrated} of {totalApplicationsToMigrate}");
                    }
                }
            }

            return new OkResult();
        }

        private string GenerateApplyData(dynamic originalApplyApplication, dynamic applySequences, dynamic applySections)
        {
            if (originalApplyApplication.ApplicationData == null)
            {
                return null;
            }

            var applyDataObject = new JObject();
            var sequences = new JArray();

            foreach (var sequence in applySequences)
            {
                var sequenceObject = new JObject();
                sequenceObject.Add("SequenceId", sequence.Id);
                sequenceObject.Add("SequenceNo", sequence.SequenceId);
                sequenceObject.Add("Status", ""); // TODO: Sequence Status
                sequenceObject.Add("IsActive", sequence.IsActive);
                sequenceObject.Add("NotRequired", sequence.NotRequired);
                sequenceObject.Add("ApprovedDate", ""); // TODO: ApprovedDate
                sequenceObject.Add("ApprovedBy", ""); // TODO: ApprovedBy

                var sections = new JArray();

                foreach (var applySection in applySections)
                {
                    if (applySection.SequenceId == sequence.SequenceId)
                    {
                        var sectionObject = new JObject();
                        sectionObject.Add("SectionId", applySection.Id);
                        sectionObject.Add("SectionNo", applySection.SectionId);
                        sectionObject.Add("Status", applySection.Status);

                        sectionObject.Add("ReviewStartDate", ""); // TODO: ReviewStartDate
                        sectionObject.Add("ReviewedBy", ""); // TODO: ReviewedBy
                        sectionObject.Add("EvaluatedDate", ""); // TODO: EvaluatedDate
                        sectionObject.Add("EvaluatedBy", ""); // TODO: EvaluatedBy

                        sectionObject.Add("NotRequired", applySection.NotRequired);
                        sectionObject.Add("RequestedFeedbackAnswered", null);
                        sections.Add(sectionObject);
                    }
                }
                sequenceObject.Add("Sections", sections);

                sequences.Add(sequenceObject);
            }

            applyDataObject.Add("Sequences", sequences);

            var applicationData = JObject.Parse(originalApplyApplication.ApplicationData);

            applyDataObject.Add("Apply", applicationData);

            applyDataObject.Add("OriginalApplicationId", originalApplyApplication.OriginalApplicationId);

            return applyDataObject.ToString();
        }

        private static string CreateFinancialGradeObject(dynamic originalApplyApplication)
        {
            if (originalApplyApplication.ApplicationData == null)
            {
                return null;
            }

            JArray financialEvidences = new JArray();
            JArray financialAnswers = null;

            if (originalApplyApplication.FinancialAnswers != null)
            {
                financialAnswers = JArray.Parse(originalApplyApplication.FinancialAnswers);
            }

            if (financialAnswers != null && financialAnswers.Count > 0)
            {
                foreach (dynamic financialAnswer in financialAnswers)
                {
                    Guid id = originalApplyApplication.Id;
                    Guid sequenceGuid = originalApplyApplication.SequenceGuid;
                    Guid sectionGuid = originalApplyApplication.SectionGuid;
                    string questionId = financialAnswer.QuestionId.Value;
                    string filename = financialAnswer.Value.Value;

                    var evidence = new JObject();
                    evidence.Add("FileName", $"{id}/{sequenceGuid}/{sectionGuid}/23/{questionId}/{filename}");

                    financialEvidences.Add(evidence);
                }
            }
            dynamic financialGrade = null;
            if (originalApplyApplication.FinancialGrade != null)
            {
                financialGrade = JObject.Parse(originalApplyApplication.FinancialGrade);
            }

            dynamic applicationData = null;
            if (originalApplyApplication.ApplicationData != null)
            {
                applicationData = JObject.Parse(originalApplyApplication.ApplicationData);
            }

            return JsonConvert.SerializeObject(
                                        new
                                        {
                                            ApplicationReference = applicationData?.ReferenceNumber.Value,
                                            SelectedGrade = financialGrade?.SelectedGrade.Value,
                                            InadequateMoreInformation = (string)financialGrade?.InadequateMoreInformation?.Value,
                                            FinancialDueDate = (DateTime?)financialGrade?.FinancialDueDate?.Value,
                                            GradedBy = (string)financialGrade?.GradedBy?.Value,
                                            GradedDateTime = (DateTime?)financialGrade?.GradedDateTime?.Value,
                                            FinancialEvidences = financialEvidences
                                        }
                                        );
        }
    }
}