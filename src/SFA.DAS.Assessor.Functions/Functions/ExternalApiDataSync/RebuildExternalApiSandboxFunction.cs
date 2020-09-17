using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
        
        [FunctionName("RebuildExternalApiSandboxFunction")]
        public async Task Run([TimerTrigger("%RebuildExternalApiSandboxFunctionSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao RebuildExternalApiSandboxFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao RebuildExternalApiSandboxFunction started");

                await _command.Execute();
                
                log.LogInformation("Epao RebuildExternalApiSandboxFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao RebuildExternalApiSandboxFunction function failed");
            }
        }
    }
}