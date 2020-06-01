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
                    log.LogInformation("Epao Importer PrintFunctionFlow timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao Importer PrintFunctionFlow started");

                await _command.Execute();

                log.LogInformation("Epao Importer PrintFunctionFlow function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao Importer PrintFunctionFlow function failed");
            }
        }
    }
}
