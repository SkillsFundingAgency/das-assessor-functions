using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintFunctionFlow
    {
        private readonly IPrintProcessCommand _command;

        public PrintFunctionFlow(IPrintProcessCommand command)
        {
            _command = command;
        }

        [FunctionName("PrintFunctionFlow")]
        public async Task Run([TimerTrigger("0 */15 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("PrintFunctionFlow has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"PrintFunctionFlow has started");
                }

                await _command.Execute();

                log.LogInformation("PrintFunctionFlow has completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "PrintFunctionFlow has failed failed");
            }
        }
    }
}
