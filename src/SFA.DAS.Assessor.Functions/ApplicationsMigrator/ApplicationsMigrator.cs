using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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
        public ActionResult<MigrationResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")]
            HttpRequest req, ILogger log)
        {

            var notMigratedApplications = new List<MigrationError>();
            int totalApplicationsToMigrate = 0;
            int applicationsMigrated = 0;
            try
            {

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

                    totalApplicationsToMigrate = applyApplications.Count;
                    log.LogTrace($"Version 2");
                    log.LogTrace($"Number of applications to Migrate: {totalApplicationsToMigrate}");

                    var applicationsProcessed = 0;
                    foreach (var originalApplyApplication in applyApplications)
                    {
                        var applyingOrganisation = _dataAccess.GetApplyingOrganisation(applyConnection, originalApplyApplication.ApplyingOrganisationId);

                        Guid? organisationId = null;
                        if (!applyingOrganisation.RoEPAOApproved)
                        {
                            organisationId = _dataAccess.CreateNewOrganisation(assessorConnection, originalApplyApplication, applyingOrganisation);
                            // Get contacts for this Apply organisation and insert them into Assessor Contacts.
                            var applyOrganisationContacts = _dataAccess.GetApplyOrganisationContacts(applyConnection, applyingOrganisation.Id);
                            foreach (var contact in applyOrganisationContacts)
                            {
                                _dataAccess.CreateContact(assessorConnection, contact, organisationId.Value);
                            }
                        }
                        else
                        {
                            organisationId = _dataAccess.GetExistingOrganisation(assessorConnection, applyingOrganisation);
                        }

                        if (organisationId != default(Guid))
                        {
                            Guid qnaApplicationId = _dataAccess.CreateQnaApplicationRecord(qnaConnection, workflowId, originalApplyApplication);

                            var applySequences = _dataAccess.GetCurrentApplyApplicationSequences(applyConnection, originalApplyApplication);

                            var applySections = _dataAccess.GetCurrentApplyApplicationSections(applyConnection, originalApplyApplication);

                            var qnaSectionQnaDatas = new List<string>();

                            foreach (var applySequence in applySequences)
                            {
                                _dataAccess.CreateQnaApplicationSequencesRecord(qnaConnection, qnaApplicationId, applySequence);

                                foreach (var applySection in applySections)
                                {
                                    if (applySection.SequenceId == applySequence.SequenceId)
                                    {
                                        applySection.QnAData = _qnaDataTranslator.Translate(applySection, applySequence, log);
                                        _dataAccess.CreateQnaApplicationSectionsRecord(qnaConnection, qnaApplicationId, applySequence, applySection);
                                        qnaSectionQnaDatas.Add(applySection.QnAData);
                                    }
                                }
                            }

                            dynamic applyDataObject = GenerateApplyData(originalApplyApplication, applySequences, applySections, assessorConnection);
                            dynamic financialGradeObject = CreateFinancialGradeObject(originalApplyApplication);

                            _dataAccess.CreateAssessorApplyRecord(assessorConnection, originalApplyApplication, qnaApplicationId, organisationId, applyDataObject, financialGradeObject);

                            var applicationData = GenerateApplicationData(qnaSectionQnaDatas, log, organisationId, originalApplyApplication, assessorConnection);

                            _dataAccess.UpdateQnaApplicationData(qnaConnection, qnaApplicationId, applicationData);

                            applicationsMigrated++;
                        }
                        else
                        {
                            notMigratedApplications.Add(new MigrationError { OriginalApplicationId = originalApplyApplication.Id, Reason = $"Organisation UkPrn: {originalApplyApplication.OrganisationUKPRN} RoEPAOApproved = true but not found in register." });
                        }

                        applicationsProcessed++;

                        if (applicationsProcessed % 10 == 0 || applicationsProcessed == totalApplicationsToMigrate)
                        {
                            log.LogTrace($"Processed {applicationsProcessed} of {totalApplicationsToMigrate}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError($"Exception in Function: {e.Message} {e.StackTrace}", e);
                if (e.InnerException != null)
                {
                    log.LogError($"Inner Exception in Function: {e.Message} {e.StackTrace}", e);
                }
                return new MigrationResult{MigrationErrors = new List<MigrationError>{new MigrationError{OriginalApplicationId = null, Reason = $"Exception in Function: {e.Message} {e.StackTrace}"}}};
            }
            var returnInformation = new MigrationResult() { 
                NumberOfApplicationsToMigrate = totalApplicationsToMigrate, 
                NumberOfApplicationsMigrated = applicationsMigrated, 
                MigrationErrors = notMigratedApplications 
            };

            log.LogInformation(JsonConvert.SerializeObject(returnInformation, Formatting.Indented));
        
            return returnInformation;
        }

    private string GenerateApplicationData(List<string> qnaSectionQnaDatas, ILogger log, Guid? organisationId, dynamic originalApplyApplication, SqlConnection assessorConnection)
    {
        JObject originalApplicationDataObj = null;
        string referenceNumber = null;
        if (originalApplyApplication.ApplicationData != null)
        {
            originalApplicationDataObj = JObject.Parse(originalApplyApplication.ApplicationData);
        }
        else
        {
            var seq = _dataAccess.GetNextAppReferenceSequence(assessorConnection);
            referenceNumber = $"AAD{seq:D6}";
        }            

        var applicationDataObject = new JObject();

        applicationDataObject.Add("OrganisationReferenceId", organisationId);
        applicationDataObject.Add("OrganisationName", originalApplyApplication.Name);
        applicationDataObject.Add("ReferenceNumber", referenceNumber ?? originalApplicationDataObj?["ReferenceNumber"]);
        applicationDataObject.Add("StandardName", originalApplicationDataObj?["StandardName"]);
        applicationDataObject.Add("StandardCode", originalApplicationDataObj?["StandardCode"]);

        applicationDataObject.Add("OrganisationType", originalApplyApplication.OrganisationType);

        applicationDataObject.Add("TradingName", (string)null);
        applicationDataObject.Add("UseTradingName", false);
        applicationDataObject.Add("ContactGivenName", (string)null);

        applicationDataObject.Add("CompanySummary", null);
        applicationDataObject.Add("CharitySummary", null);
        applicationDataObject.Add("OriginalApplicationId", originalApplyApplication.Id);

        InjectQuestionTagAnswers(qnaSectionQnaDatas, log, applicationDataObject);

        return JsonConvert.SerializeObject(applicationDataObject, Formatting.None);
    }

    private static void InjectQuestionTagAnswers(List<string> qnaSectionQnaDatas, ILogger log, JObject applicationDataObject)
    {
        foreach (var qnaDataString in qnaSectionQnaDatas)
        {
            var qnaDataObj = JObject.Parse(qnaDataString);

            foreach (JObject page in qnaDataObj["Pages"])
            {
                foreach (JObject question in page["Questions"])
                {
                    if (question.ContainsKey("QuestionTag") && !string.IsNullOrWhiteSpace((string)question["QuestionTag"]))
                    {
                        string questionId = question["QuestionId"].Value<string>();
                        InjectAnswer(log, applicationDataObject, page, question, questionId);
                    }
                    else
                    {
                        var inputObj = question["Input"];
                        if (inputObj["Type"].Value<string>() == "ComplexRadio")
                        {
                            foreach (var option in inputObj["Options"])
                            {
                                foreach (JObject furtherQuestion in option["FurtherQuestions"])
                                {
                                    if (furtherQuestion.ContainsKey("QuestionTag") && !string.IsNullOrWhiteSpace((string)furtherQuestion["QuestionTag"]))
                                    {
                                        string questionId = furtherQuestion["QuestionId"].Value<string>();
                                        InjectAnswer(log, applicationDataObject, page, furtherQuestion, questionId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void InjectAnswer(ILogger log, JObject applicationDataObject, JObject page, JObject question, string questionId)
    {
        foreach (JObject pageOfAnswers in page["PageOfAnswers"])
        {
            foreach (JObject answer in pageOfAnswers["Answers"])
            {
                if (answer["QuestionId"].Value<string>() == questionId)
                {
                    applicationDataObject.Add(question["QuestionTag"].Value<string>(), answer["Value"].Value<string>());
                }
            }
        }
    }

    private string GenerateApplyData(dynamic originalApplyApplication, dynamic applySequences, dynamic applySections, SqlConnection assessorConnection)
    {
        var applyDataObject = new JObject();

        applyDataObject.Add("OriginalApplicationId", originalApplyApplication.OriginalApplicationId);

        var sequences = new JArray();

        foreach (var sequence in applySequences)
        {
            var sequenceObject = new JObject();
            sequenceObject.Add("SequenceId", sequence.Id);
            sequenceObject.Add("SequenceNo", sequence.SequenceId);
            sequenceObject.Add("Status", sequence.Status);
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

        JObject applicationData;
        if (originalApplyApplication.ApplicationData == null)
        {
            var seq = _dataAccess.GetNextAppReferenceSequence(assessorConnection);

            var referenceNumber = $"AAD{seq:D6}";

            applicationData = JObject.Parse(JsonConvert.SerializeObject(new {
                ReferenceNumber = referenceNumber,
                StandardCode = (string)null,
                StandardReference = (string)null,
                StandardName = (string)null,
                InitSubmissions = JsonConvert.SerializeObject(new JArray()),
                InitSubmissionCount = 0,
                LatestInitSubmissionDate = (string)null,
                InitSubmissionFeedbackAddedDate = (string)null,
                InitSubmissionClosedDate = (string)null,
                StandardSubmissions = JsonConvert.SerializeObject(new JArray()),
                StandardSubmissionsCount = 0, 
                LatestStandardSubmissionDate = (string)null,
                StandardSubmissionFeedbackAddedDate = (string)null,
                StandardSubmissionClosedDate = (string)null
            }));
        }
        else
        {
            applicationData = JObject.Parse(originalApplyApplication.ApplicationData);
        }

        applyDataObject.Add("Apply", applicationData);

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
                Guid sequenceGuid = originalApplyApplication.SequenceOneGuid;
                Guid sectionGuid = originalApplyApplication.FinancialSectionGuid;
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