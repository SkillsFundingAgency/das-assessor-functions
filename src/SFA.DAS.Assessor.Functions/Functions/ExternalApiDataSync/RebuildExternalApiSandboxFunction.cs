using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.ExternalApiDataSync
{
    public class RebuildExternalApiSandboxFunction
    {
        private readonly IRebuildExternalApiSandboxCommand _command;
        private readonly ILogger<RebuildExternalApiSandboxFunction> _logger;

        public RebuildExternalApiSandboxFunction(IRebuildExternalApiSandboxCommand command, ILogger<RebuildExternalApiSandboxFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("RebuildExternalApiSandbox")]
        public async Task Run([TimerTrigger("%RebuildExternalApiSandboxTimerSchedule%", RunOnStartup = false)]TimerInfo myTimer)
        {
            await FunctionHelper.Run("RebuildExternalApiSandbox", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}