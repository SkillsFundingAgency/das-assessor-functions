using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Assessors
{
    public class AparSummaryUpdateFunction
    {
        private readonly IAparSummaryUpdateCommand _command;

        public AparSummaryUpdateFunction(IAparSummaryUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("UpdateAparSummary")]
        public async Task Run([TimerTrigger("%FunctionsOptions:UpdateAparSummaryOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"AparSummaryUpdate has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"AparSummaryUpdate has started");
                }

                await _command.Execute();

                log.LogInformation("AparSummaryUpdate has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "AparSummaryUpdate has failed");
            }
        }
    }
}
