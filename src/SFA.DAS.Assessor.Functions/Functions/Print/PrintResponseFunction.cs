using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintResponseFunction
    {
        private readonly IPrintResponseCommand _command;
        private readonly ILogger<PrintResponseFunction> _logger;

        public PrintResponseFunction(IPrintResponseCommand command, ILogger<PrintResponseFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("CertificatePrintResponse")]
        [QueueOutput(QueueNames.CertificatePrintStatusUpdate)]
        public async Task<List<CertificatePrintStatusUpdateMessage>> Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintResponseOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation("CertificatePrintResponse has started" + (myTimer.IsPastDue ? " later than scheduled" : string.Empty));

                var printStatusUpdateMessages = await _command.Execute();

                _logger.LogInformation("CertificatePrintResponse has finished");

                return printStatusUpdateMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CertificatePrintResponse has failed");
                return new List<CertificatePrintStatusUpdateMessage>();
            }
        }
    }
}
