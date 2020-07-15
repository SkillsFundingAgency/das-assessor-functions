using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardCollationImportFlow
    {
        private readonly IStandardCollationImportCommand _command;

        public StandardCollationImportFlow(IStandardCollationImportCommand command)
        {
            _command = command;
        }

        [FunctionName("StandardCollationImportFlow")]
        public async Task Run([TimerTrigger("%StandardCollationImportFlowFunctionSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"Epao StandardCollationImportFlow has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"Epao StandardCollationImportFlow has started");
                }

                await _command.Execute();

                log.LogInformation("Epao StandardCollationImportFlow has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao StandardCollationImportFlow has failed");
            }
        }
    }
}
