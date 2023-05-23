using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Providers
{
    public class RefreshProvidersFunction
    {
        private readonly IRefreshProvidersCommand _command;

        public RefreshProvidersFunction(IRefreshProvidersCommand command)
        {
            _command = command;
        }

        [FunctionName("RefreshProviders")]
        public async Task Run([TimerTrigger("%FunctionsOptions:RefreshProvidersOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"RefreshProviders has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"RefreshProviders has started");
                }

                await _command.Execute();

                log.LogInformation($"RefreshProviders has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"RefreshProviders has failed");
                throw;
            }
        }
    }
}
