using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.ExternalApiDataSync
{
    public class ExternalApiDataSyncFunction
    {
        private readonly IExternalApiDataSyncCommand _command;

        public ExternalApiDataSyncFunction(IExternalApiDataSyncCommand command)
        {
            _command = command;
        }

        [FunctionName("ExternalApiDataSyncFunction")]
        public async Task Run([TimerTrigger("%ExternalApiDataSyncFunctionSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao ExternalApiDataSyncFunctionSchedule timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao ExternalApiDataSyncFunctionSchedule started");

                await _command.Execute();

                log.LogInformation("Epao ExternalApiDataSyncFunctionSchedule function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao Importer DeliveryNotificationFunction function failed");
            }
        }
    }
}