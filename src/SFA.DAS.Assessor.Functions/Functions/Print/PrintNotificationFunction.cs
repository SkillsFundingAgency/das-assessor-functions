using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintNotificationFunction
    {
        private readonly IPrintNotificationCommand _command;

        public PrintNotificationFunction(IPrintNotificationCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintNotificationFunction")]
        public async Task Run([TimerTrigger("%FunctionsSettings:CertificatePrintNotificationFunction:Schedule%", RunOnStartup = true)]TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao CertificatePrintNotificationFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao CertificatePrintNotificationFunction started");

                _command.StorageQueue = storageQueue;
                await _command.Execute();

                log.LogInformation("Epao CertificatePrintNotificationFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao CertificatePrintNotificationFunction function failed");
            }
        }
    }
}
