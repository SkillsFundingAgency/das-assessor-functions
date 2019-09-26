using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class WorkflowMigrator
    {
        private readonly SqlConnectionStrings _connectionStrings;

        public WorkflowMigrator(IOptions<SqlConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        [FunctionName("WorkflowMigrator")]
        public IActionResult Run( [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"WorkflowMigrator - HTTP trigger function executed at: {DateTime.Now}");

            return (ActionResult) new OkObjectResult("Ok");
        }
    }
}
