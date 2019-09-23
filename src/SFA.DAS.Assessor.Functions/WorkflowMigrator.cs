using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public static class WorkflowMigrator
    {
        [FunctionName("WorkflowMigrator")]
        public static void Run( [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"HTTP trigger function executed at: {DateTime.Now}");
        }
    }
}
