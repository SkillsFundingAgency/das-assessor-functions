using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
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

        [Function("RefreshProviders")]
        public async Task Run([TimerTrigger("%FunctionsOptions:RefreshProvidersOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("RefreshProviders", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
