using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintNotificationFunction
    {
        private readonly IPrintNotificationCommand _command;

        public PrintNotificationFunction(IPrintNotificationCommand command)
        {
            _command = command;
        }

        [FunctionName("PrintNotificationFunction")]
        public async Task Run([TimerTrigger("0 */15 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao Importer PrintNotificationFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao Importer PrintNotificationFunction started");

                await _command.Execute();

                log.LogInformation("Epao Importer PrintNotificationFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao Importer PrintNotificationFunction function failed");
            }
        }
    }
}
