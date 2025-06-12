using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
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
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintResponseOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<CertificatePrintStatusUpdateMessage> storageQueue,
            ILogger log)
        {
            try
            {
                log.LogInformation("CertificatePrintResponse has started" + (myTimer.IsPastDue ? " later than scheduled" : string.Empty));

                var printStatusUpdateMessages = await _command.Execute();
                printStatusUpdateMessages?.ForEach(p => storageQueue.Add(p));

                log.LogInformation("CertificatePrintResponse has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificatePrintResponse has failed");
            }
        }
    }
}
