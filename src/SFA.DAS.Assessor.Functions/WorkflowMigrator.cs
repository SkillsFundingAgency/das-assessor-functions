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
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class WorkflowMigrator
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnectionStrings _connectionStrings;
        private readonly AssessorApiAuthentication _assessorApi;

        public WorkflowMigrator(IConfiguration configuration, IOptions<SqlConnectionStrings> connectionStrings, IOptions<AssessorApiAuthentication> assessorApiAuthenticationOptions)
        {
            _configuration = configuration;
            _connectionStrings = connectionStrings.Value;
            _assessorApi = assessorApiAuthenticationOptions.Value;
        }

        [FunctionName("WorkflowMigrator")]
        public IActionResult Run( [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")] HttpRequest req, ILogger log)
        {
            // Get config json

            // try{

            //     log.LogInformation($"ConfigurationStorageConnectionString: {_configuration["ConfigurationStorageConnectionString"]}");

            //     log.LogInformation($"Before var storageAccount = CloudStorageAccount.Parse(_configuration[ConfigurationStorageConnectionString]);");
            //     var storageAccount = CloudStorageAccount.Parse(_configuration["ConfigurationStorageConnectionString"]);
            //     log.LogInformation($"After var storageAccount = CloudStorageAccount.Parse(_configuration[ConfigurationStorageConnectionString]);");
            //     log.LogInformation($"Before var tableClient = storageAccount.CreateCloudTableClient().GetTableReference(\"Configuration\");");
            //     var tableClient = storageAccount.CreateCloudTableClient().GetTableReference("Configuration");
            //     log.LogInformation($"Before var operation = TableOperation.Retrieve<ConfigurationItem>(_configuration[\"EnvironmentName\"], $\"SFA.DAS.Assessor.Functions_1.0\");");
            //     var operation = TableOperation.Retrieve<ConfigurationItem>(_configuration["EnvironmentName"], $"SFA.DAS.AssessorFunctions_1.0");
                
            //     log.LogInformation($"Before var result = tableClient.Execute(operation).Result;");
            //     var executeResult = tableClient.Execute(operation);
                
            //     var result = executeResult.Result;

            //     log.LogInformation($"result StatusCode: {executeResult.HttpStatusCode}");
            //     log.LogInformation($"result is: {result}");

            //     log.LogInformation($"Before var configItem = (ConfigurationItem)result;");
            //     var configItem = (ConfigurationItem)result;
            //     log.LogInformation($"configItem is: {configItem}");

            //     log.LogInformation($"Before var functionsConfig = JsonConvert.DeserializeObject<FunctionsConfiguration>(configItem.Data);");
            //     var functionsConfig = JsonConvert.DeserializeObject<FunctionsConfiguration>(configItem.Data);
            // }
            // catch(Exception ex){
            //     LogException(ex, log);
            // }

            log.LogInformation($"WorkflowMigrator - HTTP trigger function executed at: {DateTime.Now}");

            log.LogInformation($"Base Address: {_assessorApi.ApiBaseAddress}"); 

            return (ActionResult) new OkObjectResult("Ok");
        }

        private void LogException(Exception exception, ILogger log)
        {
            log.LogInformation($"Error: {exception.Message} Stack: {exception.StackTrace}");
            if (exception.InnerException != null)
            {
                LogException(exception.InnerException, log);
            }
        }
    }
}
