using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardSummaryUpdateFunction
    {
        private readonly IStandardSummaryUpdateCommand _command;
        private readonly ILogger<StandardSummaryUpdateFunction> _logger;   

        public StandardSummaryUpdateFunction(IStandardSummaryUpdateCommand command, ILogger<StandardSummaryUpdateFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("StandardSummaryUpdate")]
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardSummaryUpdateOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer)
        {
            await FunctionHelper.Run("StandardSummaryUpdate", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}
