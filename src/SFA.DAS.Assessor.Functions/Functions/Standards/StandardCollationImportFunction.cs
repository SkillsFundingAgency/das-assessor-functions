using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardCollationImportFunction
    {
        private readonly IStandardCollationImportCommand _command;

        public StandardCollationImportFunction(IStandardCollationImportCommand command)
        {
            _command = command;
        }

        [FunctionName("StandardCollationImport")]
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardCollationImportOptions:Schedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"StandardCollationImport has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"StandardCollationImport has started");
                }

                await _command.Execute();

                log.LogInformation("StandardCollationImport has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "StandardCollationImport has failed");
            }
        }
    }
}
