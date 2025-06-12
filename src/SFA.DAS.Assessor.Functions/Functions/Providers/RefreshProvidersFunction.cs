using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Providers
{
    public class RefreshProvidersFunction
    {
        private readonly IRefreshProvidersCommand _command;
        private readonly ILogger<RefreshProvidersFunction> _logger;

        public RefreshProvidersFunction(IRefreshProvidersCommand command, ILogger<RefreshProvidersFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("RefreshProviders")]
        public async Task Run([TimerTrigger("%RefreshProvidersTimerSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            await FunctionHelper.Run("RefreshProviders", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}
