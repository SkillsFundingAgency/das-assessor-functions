using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;

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
        public async Task Run([QueueTrigger(EpaoDataSync.ProviderQueueName, Connection = "ConfigurationStorageConnectionString")]string message, ILogger logger)
        {
            try
            {
                logger.LogDebug($"Epao data sync dequeue provider function triggered for: {message}");

                var providerMessage = JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(message);
                await _epaoDataSyncLearnerService.ProcessLearners(providerMessage);
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
