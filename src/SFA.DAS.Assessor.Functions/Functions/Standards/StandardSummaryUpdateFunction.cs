using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;

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
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardSummaryUpdateOptions:Schedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"StandardSummaryUpdate has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"StandardSummaryUpdate has started");
                }

                await _command.Execute();

                log.LogInformation("StandardSummaryUpdate has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "StandardSummaryUpdate has failed");
            }
        }
    }
}
