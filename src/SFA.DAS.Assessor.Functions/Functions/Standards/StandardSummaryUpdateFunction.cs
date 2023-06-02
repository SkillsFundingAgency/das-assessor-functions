using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardSummaryUpdateFunction : TimerTriggerFunction
    {
        private readonly IStandardSummaryUpdateCommand _command;

        public StandardSummaryUpdateFunction(IStandardSummaryUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("StandardSummaryUpdate")]
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardSummaryUpdateOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            await base.Run("StandardSummaryUpdate", _command, myTimer, log);
        }
    }
}
