using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncEnqueueProviders
    {
        private readonly IEpaoDataSyncProviderService _epaoDataSyncProviderService;

        public EpaoDataSyncEnqueueProviders(IEpaoDataSyncProviderService epaoDataSyncProviderService)
        {
            _epaoDataSyncProviderService = epaoDataSyncProviderService;
        }

        [FunctionName("EpaoDataSyncEnqueueProviders")]
        public async Task Run([TimerTrigger("0 0 19 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger logger)
        {
            try
            {
                logger.LogInformation($"Epao data sync enqueue providers function started");
                if (myTimer.IsPastDue)
                {
                    logger.LogInformation("Epao data sync enqueue providers timer trigger is running later than scheduled");
                }

                await _epaoDataSyncProviderService.ProcessProviders();

                logger.LogInformation("Epao data sync enqueue providers function completed");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Epao data sync enqueue providers function failed");
            }
        }
    }
}
