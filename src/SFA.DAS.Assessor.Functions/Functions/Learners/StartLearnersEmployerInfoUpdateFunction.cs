using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public static class StartLearnersEmployerInfoUpdateFunction
    {
        [FunctionName("StartLearnersEmployerInfoUpdateFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue(QueueNames.StartUpdateLearnersInfo)] ICollector<string> startUpdatingLearnersQueue,
            ILogger log)
        {
            try
            {
                log.LogDebug($"StartLearnersEmployerInfoUpdateFunction has started.");

                startUpdatingLearnersQueue.Add("start processing");

                log.LogDebug($"StartLearnersEmployerInfoUpdateFunction has finished.");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"StartLearnersEmployerInfoUpdateFunction has failed.");
                throw;
            }

            return new OkObjectResult("Start Learners EmployerInfo Update Function finished");
        }
    }
}
