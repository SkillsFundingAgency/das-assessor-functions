using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncDequeueProviders
    {
        private readonly IEpaoDataSyncLearnerService _epaoDataSyncLearnerService;
        private readonly IEpaoServiceBusQueueService _epaoServiceBusQueueService;

        public EpaoDataSyncDequeueProviders(IEpaoDataSyncLearnerService epaoDataSyncLearnerService, IEpaoServiceBusQueueService storageQueueService)
        {
            _epaoDataSyncLearnerService = epaoDataSyncLearnerService;
            _epaoServiceBusQueueService = storageQueueService;
        }

        [FunctionName("EpaoDataSyncDequeueProviders")]
        public async Task Run(
            [ServiceBusTrigger(QueueNames.EpaoDataSync, Connection = "EpaoServiceBusConnectionString")] Message message, 
            int deliveryCount,
            ILogger logger)
        {
            try
            {
                logger.LogDebug($"Epao data sync dequeue provider function triggered for: {message}");

                if (Debugger.IsAttached && deliveryCount > 1)
                {
                    // when debugging a message it is possible to take longer than the default 30 second lock 
                    // duration to be processed and it will then be received again by the function; recommendation is
                    // to set the default lock duration to the maximum of 5 minutes when creating the service bus queue
                    // in the portal
                    Debugger.Break();
                }

                var providerMessage = _epaoServiceBusQueueService.DeserializeMessage(message);
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
