using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Apar
{
    public class AparSummaryUpdateFunction
    {
        private readonly IAparSummaryUpdateCommand _command;

        public AparSummaryUpdateFunction(IAparSummaryUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("AparSummaryUpdate")]
        public async Task Run([TimerTrigger("%FunctionsOptions:AparSummaryUpdateOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("AparSummaryUpdate", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
