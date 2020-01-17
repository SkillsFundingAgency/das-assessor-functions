using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncDequeueProviders
    {
        private readonly IEpaoDataSyncLearnerService _epaoDataSyncLearnerService;

        public EpaoDataSyncDequeueProviders(IEpaoDataSyncLearnerService epaoDataSyncLearnerService)
        {
            _epaoDataSyncLearnerService = epaoDataSyncLearnerService;
        }

        [FunctionName("EpaoDataSyncDequeueProviders")]
        public async Task Run(
            [QueueTrigger(QueueNames.EpaoDataSync, Connection = "ConfigurationStorageConnectionString")]string message,
            [Queue(QueueNames.EpaoDataSync, Connection = "ConfigurationStorageConnectionString")]CloudQueue epaoDataSyncQueue,
            ILogger logger)
        {
            try
            {
                logger.LogDebug($"Epao data sync dequeue provider function triggered for: {message}");

                var providerMessage = JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(message);
                var nextPageProviderMessage = await _epaoDataSyncLearnerService.ProcessLearners(providerMessage);
                if (nextPageProviderMessage != null)
                {
                    await epaoDataSyncQueue.AddMessageAsync(
                        new CloudQueueMessage(JsonConvert.SerializeObject(nextPageProviderMessage)));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Epao data sync dequeue providers function failed for {message}");
                throw;
            }

            logger.LogDebug("Epao data sync dequeue provider function completed");
        }
    }
}
