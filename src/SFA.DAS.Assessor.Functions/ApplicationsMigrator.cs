using System;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

                var applyApplications = applyConnection.Query("SELECT * FROM Applications WHERE ApplicationStatus NOT IN ('Approved','Rejected')");
                foreach (var originalApplyApplication in applyApplications)
                {
                    var qnaApplicationId = Guid.NewGuid();

                    // Create Qna Applications record
                    qnaConnection.Execute(@"INSERT INTO Applications (Id, WorkflowId, Reference, CreatedAt, ApplicationStatus, ApplicationData) 
                                            VALUES (@Id, @WorkflowId, 'Migrated from Apply', @CreatedAt, @ApplicationStatus, '')",
                                            new {Id = qnaApplicationId, WorkflowId = workflowId, originalApplyApplication.CreatedAt, originalApplyApplication.ApplicationStatus});

                    var applySequences =  applyConnection.Query("SELECT * FROM ApplicationSequences WHERE ApplicationId = @ApplicationId", new {ApplicationId = originalApplyApplication.Id});

                    foreach (var applySequence in applySequences) 
                    {
                        qnaConnection.Execute("INSERT INTO ApplicationSequences (Id, ApplicationId, SequenceNo, IsActive) VALUES (@Id, @ApplicationId, @SequenceNo, @IsActive)",
                                                new {Id = applySequence.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, IsActive = applySequence.IsActive});

                        var applySections =  applyConnection.Query("SELECT * FROM ApplicationSections WHERE ApplicationId = @ApplicationId AND SequenceId = @SequenceId", 
                                                                    new {ApplicationId = originalApplyApplication.Id, SequenceId = applySequence.SequenceId});
                        foreach (var applySection in applySections)
                        {
                            qnaConnection.Execute(@"INSERT INTO ApplicationSections (Id, ApplicationId, SequenceNo, SectionNo, QnaData, Title, LinkTitle, DisplayType, SequenceId) 
                                                    VALUES (@Id, @ApplicationId, @SequenceNo, @SectionNo, @QnaData, @Title, @LinkTitle, @DisplayType, @SequenceId)",
                                                    new {Id = applySection.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, SectionNo = applySection.SectionId, QnaData = applySection.QnAData, Title = applySection.Title, LinkTitle = applySection.LinkTitle, DisplayType = applySection.DisplayType, SequenceId = applySequence.Id});
                        }
                    }
                }
            }


            // For each existing in-flight Application in Apply.
            
            // Convert ApplicationData
            // Create Qna ApplicationSequences record
            // Create Qna ApplicationSections record
            // Modify QnAData to new format.
            // Create Assessor Apply record.
            // Create Organisation record if it ain't there.
            

            return new OkResult();
        }
    }
}