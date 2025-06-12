using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardSummaryUpdateFunction
    {
        private readonly IStandardSummaryUpdateCommand _command;

        public StandardSummaryUpdateFunction(IStandardSummaryUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("StandardSummaryUpdate")]
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardSummaryUpdateOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("StandardSummaryUpdate", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
