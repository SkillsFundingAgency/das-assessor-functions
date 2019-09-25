using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Microsoft.Extensions.Http;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class WorkflowMigrator
    {
        private readonly IOptions<AssessorApiAuthentication> _configuration;
        private readonly SqlConnectionStrings _connectionStrings;

        public WorkflowMigrator(IOptions<AssessorApiAuthentication> configuration, IOptions<SqlConnectionStrings> connectionStrings)
        {
            _configuration = configuration;
            _connectionStrings = connectionStrings.Value;
        }

        [FunctionName("WorkflowMigrator")]
        public IActionResult Run( [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"WorkflowMigrator - HTTP trigger function executed at: {DateTime.Now}");

            log.LogInformation($"Base Address: {_configuration.Value.ApiBaseAddress}");

            return (ActionResult) new OkObjectResult("Ok");
        }
    }
}
