using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.Assessor.Functions.Infrastructure;


namespace SFA.DAS.Assessor.Functions
{
    public class ApplicationsMigrator
    {
        private readonly SqlConnectionStrings _connectionStrings;

        public ApplicationsMigrator(IOptions<SqlConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }
        
        [FunctionName("ApplicationsMigrator")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "applicationsMigrator")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation($"ApplicationsMigrator - HTTP trigger function executed at: {DateTime.Now}");

            using (var applyConnection = new SqlConnection(_connectionStrings.Apply))
            using (var qnaConnection = new SqlConnection(_connectionStrings.QnA))
            using (var assessorConnection = new SqlConnection(_connectionStrings.Assessor))
            {
                var workflowId = qnaConnection.QuerySingleOrDefault<Guid?>("SELECT Id FROM Workflows WHERE Type = 'EPAO'");
                if (workflowId is null)
                {
                    throw new ApplicationException("Workflow of Type 'EPAO' not found.");
                }

                var applyApplications = GetCurrentApplyApplications(applyConnection);

                foreach (var originalApplyApplication in applyApplications)
                {
                    //originalApplyApplication.AnswersArray = JArray.Parse(originalApplyApplication.Answers);

                    Guid qnaApplicationId = CreateQnaApplicationRecord(qnaConnection, workflowId, originalApplyApplication);

                    var applySequences = GetCurrentApplyApplicationSequences(applyConnection, originalApplyApplication);

                    foreach (var applySequence in applySequences)
                    {
                        CreateQnaApplicationSequencesRecord(qnaConnection, qnaApplicationId, applySequence);

                        var applySections = GetCurrentApplyApplicationSections(applyConnection, originalApplyApplication, applySequence);

                        foreach (var applySection in applySections)
                        {
                            CreateQnaApplicationSectionsRecord(qnaConnection, qnaApplicationId, applySequence, applySection);
                        }
                    }

                    var applyingOrganisation = applyConnection.QuerySingle("SELECT * FROM Organisations WHERE Id = @Id", new { Id = originalApplyApplication.ApplyingOrganisationId });

                    Guid organisationId;
                    if (!applyingOrganisation.RoEPAOApproved)
                    {
                        organisationId = CreateNewOrganisation(assessorConnection, originalApplyApplication);
                    }
                    else
                    {
                        organisationId = GetExistingOrganisation(assessorConnection, applyingOrganisation);
                    }

                    // Create Assessor Apply record.
                    assessorConnection.Execute(@"INSERT INTO Apply (Id, ApplicationId, OrganisationId, ApplicationStatus, ReviewStatus, ApplyData, FinancialReviewStatus, FinancialGrade, StandardCode, CreatedAt, CreatedBy) 
                                                VALUES (NEWID(), @ApplicationId, @OrganisationId, @ApplicationStatus, @ReviewStatus, @ApplyData, @FinancialReviewStatus, @FinancialGrade, @StandardCode, @CreatedAt, @CreatedBy)", new
                    {
                        ApplicationId = qnaApplicationId,
                        OrganisationId = organisationId,
                        ApplicationStatus = originalApplyApplication.ApplicationStatus,
                        ReviewStatus = "", //TODO: ReviewStatus
                        ApplyData = "", // TODO: ApplyData
                        FinancialReviewStatus = "", // TODO: FinancialReviewStatus
                        FinancialGrade = CreateFinancialGradeObject(originalApplyApplication),
                        StandardCode = "",
                        CreatedAt = originalApplyApplication.CreatedAt,
                        CreatedBy = originalApplyApplication.CreatedBy
                    });

                    // Convert ApplicationData
                    // Modify QnAData to new format.
                }
            }
            return new OkResult();
        }

        private static string CreateFinancialGradeObject(dynamic originalApplyApplication)
        {
            JArray financialEvidences = null;
            JArray financialAnswers = JArray.Parse(originalApplyApplication.FinancialAnswers);
            if(financialAnswers != null && financialAnswers.Count > 0)
            {
                financialEvidences = new JArray();
                foreach(dynamic financialAnswer in financialAnswers)
                {
                    Guid id = originalApplyApplication.Id;
                    Guid sequenceGuid = originalApplyApplication.SequenceGuid;
                    Guid sectionGuid = originalApplyApplication.SectionGuid;
                    string questionId = financialAnswer.QuestionId.Value;
                    string filename = financialAnswer.Value.Value;

                    var evidence = new JObject();
                    evidence.Add("FileName", $"{id}/{sequenceGuid}/{sectionGuid}/23/{questionId}/{filename}");

                    financialEvidences.Add(evidence);

                    // financialEvidences.Add(new {
                    //     FileName = $"{id}/{sequenceGuid}/{sectionGuid}/23/{questionId}/{filename}"
                    // });
                }
            }


            dynamic financialGrade = null;
            if(originalApplyApplication.FinancialGrade != null)
            {
                financialGrade = JObject.Parse(originalApplyApplication.FinancialGrade);
            }            
            
            dynamic applicationData = null;
            if(originalApplyApplication.ApplicationData != null)
            {
                applicationData = JObject.Parse(originalApplyApplication.ApplicationData);
            }

            return JsonConvert.SerializeObject(
                                        new
                                        {
                                            ApplicationReference = applicationData?.ReferenceNumber.Value,
                                            SelectedGrade = financialGrade?.SelectedGrade.Value,
                                            InadequateMoreInformation = financialGrade?.InadequateMoreInformation.Value,
                                            FinancialDueDate = financialGrade?.FinancialDueDate?.Value,
                                            GradedBy = financialGrade?.GradedBy.Value,
                                            GradedDateTime = financialGrade?.GradedDateTime.Value,
                                            FinancialEvidences = financialEvidences
                                        }
                                        );
        }

        private static void CreateQnaApplicationSectionsRecord(SqlConnection qnaConnection, Guid qnaApplicationId, dynamic applySequence, dynamic applySection)
        {
            qnaConnection.Execute(@"INSERT INTO ApplicationSections (Id, ApplicationId, SequenceNo, SectionNo, QnaData, Title, LinkTitle, DisplayType, SequenceId) 
                                                    VALUES (@Id, @ApplicationId, @SequenceNo, @SectionNo, @QnaData, @Title, @LinkTitle, @DisplayType, @SequenceId)",
                                                                new { Id = applySection.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, SectionNo = applySection.SectionId, QnaData = applySection.QnAData, Title = applySection.Title, LinkTitle = applySection.LinkTitle, DisplayType = applySection.DisplayType, SequenceId = applySequence.Id });
        }

        private static IEnumerable<dynamic> GetCurrentApplyApplicationSections(SqlConnection applyConnection, dynamic originalApplyApplication, dynamic applySequence)
        {
            return applyConnection.Query("SELECT * FROM ApplicationSections WHERE ApplicationId = @ApplicationId AND SequenceId = @SequenceId",
                                                        new { ApplicationId = originalApplyApplication.Id, SequenceId = applySequence.SequenceId });
        }

        private static void CreateQnaApplicationSequencesRecord(SqlConnection qnaConnection, Guid qnaApplicationId, dynamic applySequence)
        {
            // Create Qna ApplicationSequences record
            qnaConnection.Execute("INSERT INTO ApplicationSequences (Id, ApplicationId, SequenceNo, IsActive) VALUES (@Id, @ApplicationId, @SequenceNo, @IsActive)",
                                    new { Id = applySequence.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, IsActive = applySequence.IsActive });
        }

        private static IEnumerable<dynamic> GetCurrentApplyApplicationSequences(SqlConnection applyConnection, dynamic originalApplyApplication)
        {
            return applyConnection.Query("SELECT * FROM ApplicationSequences WHERE ApplicationId = @ApplicationId", new { ApplicationId = originalApplyApplication.Id });
        }

        private static Guid CreateQnaApplicationRecord(SqlConnection qnaConnection, Guid? workflowId, dynamic originalApplyApplication)
        {
            var qnaApplicationId = Guid.NewGuid();

            // Create Qna Applications record
            qnaConnection.Execute(@"INSERT INTO Applications (Id, WorkflowId, Reference, CreatedAt, ApplicationStatus, ApplicationData) 
                                            VALUES (@Id, @WorkflowId, 'Migrated from Apply', @CreatedAt, @ApplicationStatus, '')",
                                    new { Id = qnaApplicationId, WorkflowId = workflowId, originalApplyApplication.CreatedAt, originalApplyApplication.ApplicationStatus });
            return qnaApplicationId;
        }

        private static IEnumerable<dynamic> GetCurrentApplyApplications(SqlConnection applyConnection)
        {
            return applyConnection.Query(@"SELECT *, JSON_Value(ApplicationData, '$.StandardCode') AS StandardCode, 
                                                                JSON_QUERY(ApplicationSections.QnaData, '$.FinancialApplicationGrade') AS FinancialGrade,
                                                                JSON_QUERY(ApplicationSections.QnaData, '$.Pages[0].PageOfAnswers[0].Answers') AS FinancialAnswers,
																ApplicationSections.Id AS SectionGuid,
																ApplicationSequences.Id AS SequenceGuid,
																Organisations.Name,
																Organisations.OrganisationDetails
                                                                FROM Applications 
                                                                INNER JOIN ApplicationSections ON Applications.Id = ApplicationSections.ApplicationId
																INNER JOIN ApplicationSequences ON Applications.Id = ApplicationSequences.ApplicationId AND ApplicationSequences.SequenceId = 2
																INNER JOIN Organisations ON Organisations.Id = Applications.ApplyingOrganisationId
                                                                WHERE ApplicationStatus NOT IN ('Approved','Rejected') AND ApplicationSections.SectionId = 3");
        }

        private static Guid GetExistingOrganisation(SqlConnection assessorConnection, dynamic applyingOrganisation)
        {
            return assessorConnection.QuerySingle<Guid>("SELECT Id FROM Organisations WHERE EndPointAssessorUkprn = @ukprn", new { ukprn = applyingOrganisation.OrganisationUKPRN });
        }

        private static Guid CreateNewOrganisation(SqlConnection assessorConnection, dynamic originalApplyApplication)
        {
            Guid organisationId;
            string nextEpaOrgId = GetNextEpaOrgId(assessorConnection);

            organisationId = Guid.NewGuid();
            assessorConnection.Execute(@"INSERT INTO Organisations (Id, CreatedAt, EndPointAssessorName, EndPointAssessorOrganisationId, EndPointAssessorUkprn, PrimaryContact, Status, OrganisationData, ApiEnabled) 
                                                    VALUES (@Id, @CreatedAt, @EndPointAssessorName, @EndPointAssessorOrganisationId, @EndPointAssessorUkprn, @PrimaryContact, 'Applying', @OrganisationData, 0)",
                                        new
                                        {
                                            Id = organisationId,
                                            CreatedAt = originalApplyApplication.CreatedAt,
                                            EndPointAssessorName = originalApplyApplication.Name,
                                            EndPointAssessorOrganisationId = nextEpaOrgId,
                                            EndPointAssessorUkPrn = "",
                                            PrimaryContact = "",
                                            OrganisationData = originalApplyApplication.OrganisationDetails
                                        });
            return organisationId;
        }

        private static string GetNextEpaOrgId(SqlConnection assessorConnection)
        {
            var sqlToGetHighestOrganisationId = "select max(EndPointAssessorOrganisationId) OrgId from organisations where EndPointAssessorOrganisationId like 'EPA%' " +
                                                            " and isnumeric(replace(EndPointAssessorOrganisationId,'EPA','')) = 1";
            var highestEpaOrgId = assessorConnection.ExecuteScalar<string>(sqlToGetHighestOrganisationId);

            var nextEpaOrgId = int.TryParse(highestEpaOrgId.Replace("EPA", string.Empty), out int currentIntValue)
                ? $@"EPA{currentIntValue + 1:D4}" :
                string.Empty;
            return nextEpaOrgId;
        }
    }
}