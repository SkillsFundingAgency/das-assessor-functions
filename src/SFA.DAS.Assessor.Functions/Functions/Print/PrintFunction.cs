using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintFunction
    {
        private readonly IPrintCommand _command;

        public PrintFunction(IPrintCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintFunction")]
        public async Task Run([TimerTrigger("%FunctionsSettings:CertificatePrintFunction:Schedule%", RunOnStartup = true)]TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("CertificatePrintFunction has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"CertificatePrintFunction has started");
                }

                _command.StorageQueue = storageQueue;
                await _command.Execute();

                log.LogInformation("CertificatePrintFunction has completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificatePrintFunction has failed");
            }
        }
    }
}
