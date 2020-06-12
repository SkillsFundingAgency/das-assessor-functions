using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.EpaoDataSync
{
    public class EpaoDataSyncDequeueProvidersFunctionFlow
    {
        private readonly IEpaoDataSyncDequeueProvidersCommand _command;

        public EpaoDataSyncDequeueProvidersFunctionFlow(IEpaoDataSyncDequeueProvidersCommand command)
        {
            _command = command;
        }

        [FunctionName("EpaoDataSyncDequeueProviders")]
        public async Task Run(
            [QueueTrigger(QueueNames.EpaoDataSync, Connection = "ConfigurationStorageConnectionString")]string message,
            [Queue(QueueNames.EpaoDataSync, Connection = "ConfigurationStorageConnectionString")]CloudQueue epaoDataSyncQueue,
            ILogger logger)
        {
            try
            {
                logger.LogDebug($"Epao data sync dequeue provider function started for {message}");

                _command.StorageQueue = new StorageQueue(epaoDataSyncQueue);
                await _command.Execute(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Epao data sync dequeue providers function failed for {message}");
                throw;
            }

            logger.LogDebug($"Epao data sync dequeue provider function completed for {message}");
        }
    }
}
