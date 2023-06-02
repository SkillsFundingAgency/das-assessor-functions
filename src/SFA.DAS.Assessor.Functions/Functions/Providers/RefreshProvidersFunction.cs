using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Providers
{
    public class RefreshProvidersFunction : TimerTriggerFunction
    {
        private readonly IRefreshProvidersCommand _command;

        public RefreshProvidersFunction(IRefreshProvidersCommand command)
        {
            _command = command;
        }

        [FunctionName("RefreshProviders")]
        public async Task Run([TimerTrigger("%FunctionsOptions:RefreshProvidersOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await base.Run("RefreshProviders", _command, myTimer, log);
        }
    }
}
