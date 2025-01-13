using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.ExternalApiDataSync
{
    public class RebuildExternalApiSandboxFunction
    {
        private readonly IRebuildExternalApiSandboxCommand _command;

        public RebuildExternalApiSandboxFunction(IRebuildExternalApiSandboxCommand command)
        {
            _command = command;
        }
        
        [Function("RebuildExternalApiSandbox")]
        public async Task Run([TimerTrigger("%FunctionsOptions:RebuildExternalApiSandboxOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("RebuildExternalApiSandbox", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}