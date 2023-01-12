using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;
using System;
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

        [FunctionName("RebuildExternalApiSandbox")]
        public async Task Run([TimerTrigger("%FunctionsOptions:RebuildExternalApiSandboxOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"RebuildExternalApiSandbox has started later than scheduled");
                }

                log.LogInformation($"RebuildExternalApiSandbox has started");

                await _command.Execute();

                log.LogInformation("RebuildExternalApiSandbox has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "RebuildExternalApiSandbox has failed");
            }
        }
    }
}