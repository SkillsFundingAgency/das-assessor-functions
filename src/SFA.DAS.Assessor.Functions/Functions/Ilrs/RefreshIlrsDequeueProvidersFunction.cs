using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsDequeueProvidersFunction
    {
        private readonly IRefreshIlrsDequeueProvidersCommand _command;

        public RefreshIlrsDequeueProvidersFunction(IRefreshIlrsDequeueProvidersCommand command)
        {
            _command = command;
        }

        [FunctionName("RefreshIlrsDequeueProviders")]
        public async Task Run(
            [QueueTrigger(QueueNames.RefreshIlrs)] string message,
            [Queue(QueueNames.RefreshIlrs)] ICollector<string> refreshIlrsQueue,
            ILogger log)
        {
            try
            {
                log.LogDebug($"RefreshIlrsDequeueProviders has started for {message}");

                _command.StorageQueue = refreshIlrsQueue;
                await _command.Execute(message);

                log.LogDebug($"RefreshIlrsDequeueProviders has finished for {message}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"RefreshIlrsDequeueProviders has failed for {message}");
                throw;
            }
        }
    }
}
