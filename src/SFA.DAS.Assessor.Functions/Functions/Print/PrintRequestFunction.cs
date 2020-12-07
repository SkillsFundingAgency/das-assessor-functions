using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintRequestFunction
    {
        private readonly IPrintRequestCommand _command;

        public PrintRequestFunction(IPrintRequestCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintRequest")]
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintRequestOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<CertificatePrintStatusUpdateMessage> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("CertificatePrintRequest has started later than scheduled");
                }

                log.LogInformation($"CertificatePrintRequest started");

                var printStatusUpdateMessages = await _command.Execute();
                printStatusUpdateMessages?.ForEach(p => storageQueue.Add(p));

                log.LogInformation("CertificatePrintRequest finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificatePrintRequest failed");
            }
        }
    }
}
