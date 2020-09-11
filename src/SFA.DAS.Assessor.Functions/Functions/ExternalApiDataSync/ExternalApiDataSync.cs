using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync;

namespace SFA.DAS.Assessor.Functions.Functions.ExternalApiDataSync
{
    public static class ExternalApiDataSync
    {
        [FunctionName("ExternalApiDataSync")]
        public static async Task Run([TimerTrigger("0 0 0 1 * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"ExternalApiDataSync function executed at: {DateTime.Now}");

            Bootstrapper.StartUp(log, context);
            var command = Bootstrapper.Container.GetInstance<ExternalApiDataSyncCommand>();
            await command.Execute();
        }
    }
}
