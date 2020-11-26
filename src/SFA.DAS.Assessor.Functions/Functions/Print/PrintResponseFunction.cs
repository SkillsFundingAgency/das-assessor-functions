using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintResponseFunction
    {
        private readonly IPrintResponseCommand _command;

        public PrintResponseFunction(IPrintResponseCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintResponse")]
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintResponseOptions:Schedule%", RunOnStartup = true)] TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("CertificatePrintResponse has started later than scheduled");
                }

                log.LogInformation($"CertificatePrintResponse started");

                await _command.Execute(storageQueue);

                log.LogInformation("CertificatePrintResponse finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificatePrintResponse failed");
            }
        }
    }
}
