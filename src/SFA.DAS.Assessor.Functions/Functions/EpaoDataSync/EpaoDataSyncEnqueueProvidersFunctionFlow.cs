using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.EpaoDataSync
{
    public class EpaoDataSyncEnqueueProvidersFunctionFlow
    {
        private readonly IEpaoDataSyncEnqueueProvidersCommand _command;

        public EpaoDataSyncEnqueueProvidersFunctionFlow(IEpaoDataSyncEnqueueProvidersCommand command)
        {
            _command = command;
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

                _command.StorageQueue = new StorageQueue(epaoDataSyncQueue);
                await _command.Execute();

                logger.LogInformation("Epao data sync enqueue providers function completed");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Epao data sync enqueue providers function failed");
            }
        }
    }
}
