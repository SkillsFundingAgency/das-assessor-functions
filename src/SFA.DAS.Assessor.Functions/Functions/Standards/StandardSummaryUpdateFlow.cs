using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardSummaryUpdateFlow
    {
        private readonly IStandardSummaryUpdateCommand _command;

        public StandardSummaryUpdateFlow(IStandardSummaryUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("StandardSummaryUpdateFlow")]
        public async Task Run([TimerTrigger("%StandardSummaryUpdateFlowFunctionSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"Epao StandardSummaryUpdateFlow has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"Epao StandardSummaryUpdateFlow has started");
                }

                await _command.Execute();

                log.LogInformation("Epao StandardSummaryUpdateFlow has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao StandardSummaryUpdateFlow has failed");
            }
        }
    }
}
