using System;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

                var applyApplications = applyConnection.Query(@"SELECT *, JSON_Value(ApplicationData, '$.StandardCode') AS StandardCode, 
                                                                JSON_QUERY(ApplicationSections.QnaData, '$.FinancialApplicationGrade') AS FinancialGrade,
                                                                JSON_QUERY(ApplicationSections.QnaData, '$.Pages[0].PageOfAnswers[0].Answers') AS Answers
                                                                FROM Applications 
                                                                INNER JOIN ApplicationSections ON Applications.Id = ApplicationSections.ApplicationId
                                                                WHERE ApplicationStatus NOT IN ('Approved','Rejected') AND ApplicationSections.SectionId = 3");
                
                foreach (var originalApplyApplication in applyApplications)
                {
                    originalApplyApplication.AnswersArray = JArray.Parse(originalApplyApplication.Answers);
                    


                    var qnaApplicationId = Guid.NewGuid();

                    // Create Qna Applications record
                    qnaConnection.Execute(@"INSERT INTO Applications (Id, WorkflowId, Reference, CreatedAt, ApplicationStatus, ApplicationData) 
                                            VALUES (@Id, @WorkflowId, 'Migrated from Apply', @CreatedAt, @ApplicationStatus, '')",
                                            new {Id = qnaApplicationId, WorkflowId = workflowId, originalApplyApplication.CreatedAt, originalApplyApplication.ApplicationStatus});

                    var applySequences =  applyConnection.Query("SELECT * FROM ApplicationSequences WHERE ApplicationId = @ApplicationId", new {ApplicationId = originalApplyApplication.Id});

                    foreach (var applySequence in applySequences) 
                    {
                        // Create Qna ApplicationSequences record
                        qnaConnection.Execute("INSERT INTO ApplicationSequences (Id, ApplicationId, SequenceNo, IsActive) VALUES (@Id, @ApplicationId, @SequenceNo, @IsActive)",
                                                new {Id = applySequence.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, IsActive = applySequence.IsActive});

                        var applySections =  applyConnection.Query("SELECT * FROM ApplicationSections WHERE ApplicationId = @ApplicationId AND SequenceId = @SequenceId", 
                                                                    new {ApplicationId = originalApplyApplication.Id, SequenceId = applySequence.SequenceId});
                        foreach (var applySection in applySections)
                        {
                            // Create Qna ApplicationSections record
                            qnaConnection.Execute(@"INSERT INTO ApplicationSections (Id, ApplicationId, SequenceNo, SectionNo, QnaData, Title, LinkTitle, DisplayType, SequenceId) 
                                                    VALUES (@Id, @ApplicationId, @SequenceNo, @SectionNo, @QnaData, @Title, @LinkTitle, @DisplayType, @SequenceId)",
                                                    new {Id = applySection.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, SectionNo = applySection.SectionId, QnaData = applySection.QnAData, Title = applySection.Title, LinkTitle = applySection.LinkTitle, DisplayType = applySection.DisplayType, SequenceId = applySequence.Id});
                        }
                    }

                    var applyingOrganisation = applyConnection.QuerySingle("SELECT RoEPAOApproved", new {Id = originalApplyApplication.ApplyingOrganisationId});

                    Guid organisationId;
                    if (!applyingOrganisation.RoEPAOApproved)
                    {
                        // Create Organisation record if RoEPAOApproved is false
                        // Generate new EPAOrgId

                        string nextEpaOrgId = GetNextEpaOrgId(assessorConnection);

                        organisationId = Guid.NewGuid();
                        assessorConnection.Execute(@"INSERT INTO Organisation (Id, CreatedAt, EndPointAssessorName, EndPointAssessorOrganisationId, EndPointAssessorUkprn, PrimaryContact, Status, OrganisationData, ApiEnabled) 
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
                    }
                    else
                    {
                        // Get Organisation Id from existing Assessor Org record.
                        organisationId = assessorConnection.QuerySingle<Guid>("SELECT Id FROM Organisations WHERE EndPointAssessorUkprn = @ukprn", new{ukprn = applyingOrganisation.OrganisationUKPRN});
                    }
                    
                    // Create Assessor Apply record.
                    assessorConnection.Execute(@"INSERT INTO Apply (Id, ApplicationId, OrganisationId, ApplicationStatus, ReviewStatus, ApplyData, FinancialReviewStatus, FinancialGrade, StandardCode, CreatedAt, CreatedBy) 
                                                VALUES (NEWID(), @ApplicationId, @OrganisationId, @ApplicationStatus, @ReviewStatus, @ApplyData, @FinancialReviewStatus, @FinancialGrade, @StandardCode, @CreatedAt, @CreatedBy)", new {
                        ApplicationId = qnaApplicationId,
                        OrganisationId = organisationId,
                        ApplicationStatus = originalApplyApplication.ApplicationStatus,
                        ReviewStatus = "", //TODO: ReviewStatus
                        ApplyData = "", // TODO: ApplyData
                        FinancialReviewStatus = "", // TODO: FinancialReviewStatus
                        FinancialGrade = "",
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