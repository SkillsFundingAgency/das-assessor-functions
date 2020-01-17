using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncEnqueueProviders
    {
        private readonly IEpaoDataSyncProviderService _epaoDataSyncProviderService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public EpaoDataSyncEnqueueProviders(IEpaoDataSyncProviderService epaoDataSyncProviderService, IDateTimeHelper dateTimeHelper)
        {
            _epaoDataSyncProviderService = epaoDataSyncProviderService;
            _dateTimeHelper = dateTimeHelper;
        }

        [FunctionName("EpaoDataSyncEnqueueProviders")]
        public async Task Run([TimerTrigger("0 0 19 * * *", RunOnStartup = true)]TimerInfo myTimer,
            [Queue(QueueNames.EpaoDataSync, Connection = "ConfigurationStorageConnectionString")]CloudQueue epaoDataSyncQueue,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"Epao data sync enqueue providers function started");
                if (myTimer.IsPastDue)
                {
                    logger.LogInformation("Epao data sync enqueue providers timer trigger is running later than scheduled");
                }

                var output = await _epaoDataSyncProviderService.ProcessProviders();
                foreach (var message in output)
                {                    
                    await epaoDataSyncQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
                }

                await _epaoDataSyncProviderService.SetLastRunDateTime(_dateTimeHelper.DateTimeNow);

                logger.LogInformation("Epao data sync enqueue providers function completed");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Epao data sync enqueue providers function failed");
            }
        }
    }
}
