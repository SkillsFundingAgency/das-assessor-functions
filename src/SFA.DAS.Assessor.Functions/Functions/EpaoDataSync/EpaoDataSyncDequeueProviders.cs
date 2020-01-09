using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncDequeueProviders
    {
        private readonly IEpaoDataSyncLearnerService _epaoDataSyncLearnerService;
        private readonly IStorageQueueService _storageQueueService;

        public EpaoDataSyncDequeueProviders(IEpaoDataSyncLearnerService epaoDataSyncLearnerService, IStorageQueueService storageQueueService)
        {
            _epaoDataSyncLearnerService = epaoDataSyncLearnerService;
            _storageQueueService = storageQueueService;
        }

        [FunctionName("EpaoDataSyncDequeueProviders")]
        public async Task Run([QueueTrigger(EpaoDataSync.ProviderQueueName, Connection = "ConfigurationStorageConnectionString")]string message, ILogger logger)
        {
            try
            {
                logger.LogDebug($"Epao data sync dequeue provider function triggered for: {message}");

                var providerMessage = _storageQueueService.DeserializeMessage(message);
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
