using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsDequeueProvidersFunction
    {
        private readonly IRefreshIlrsDequeueProvidersCommand _command;
        private readonly ILogger<RefreshIlrsDequeueProvidersFunction> _logger;

        public RefreshIlrsDequeueProvidersFunction(
            IRefreshIlrsDequeueProvidersCommand command,
            ILogger<RefreshIlrsDequeueProvidersFunction> logger)
        {
            _command = command; 
            _logger = logger;
        }

        [Function("RefreshIlrsDequeueProviders")]
        public async Task Run(
            [QueueTrigger(QueueNames.RefreshIlrs)] string message)
        {
            try
            {
                _logger.LogDebug($"RefreshIlrsDequeueProviders has started for {message}");

                await _command.Execute(message);

                _logger.LogDebug($"RefreshIlrsDequeueProviders has finished for {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RefreshIlrsDequeueProviders has failed for {message}");
                throw;
            }
        }
    }
}
