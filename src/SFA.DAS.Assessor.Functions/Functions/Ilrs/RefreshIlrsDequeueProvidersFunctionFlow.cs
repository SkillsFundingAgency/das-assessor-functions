using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsDequeueProvidersFunctionFlow
    {
        private readonly IRefreshIlrsDequeueProvidersCommand _command;

        public RefreshIlrsDequeueProvidersFunctionFlow(IRefreshIlrsDequeueProvidersCommand command)
        {
            _command = command;
        }

        [FunctionName("RefreshIlrsDequeueProviders")]
        public async Task Run(
            [QueueTrigger(QueueNames.RefreshIlrs, Connection = "StorageAccountConnectionString")]string message,
            [Queue(QueueNames.RefreshIlrs, Connection = "StorageAccountConnectionString")]CloudQueue refreshIlrsQueue,
            ILogger log)
        {
            try
            {
                log.LogDebug($"Epao RefreshIlrsDequeueProviders has started for {message}");

                _command.StorageQueue = new StorageQueue(refreshIlrsQueue);
                await _command.Execute(message);

                log.LogDebug($"Epao RefreshIlrsDequeueProviders has completed for {message}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Epao RefreshIlrsDequeueProviders has failed for {message}");
                throw;
            }
        }
    }
}
