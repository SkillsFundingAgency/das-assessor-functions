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
                var applications = applyConnection.Query("SELECT * FROM Applications WHERE ApplicationStatus NOT IN ('Approved','Rejected')");
                foreach (var appl in applications)
                {
                    string id = appl.Id.ToString();
                    log.LogInformation(id);
                }
            }


            // For each existing in-flight Application in Apply.
            // Create Qna Applications record
            // Convert ApplicationData
            // Create Qna ApplicationSequences record
            // Create Qna ApplicationSections record
            // Create Assessor Apply record.
            // Create Organisation record if it ain't there.
            

            return new OkResult();
        }
    }
}