using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintRequestFunction
    {
        private readonly IPrintRequestCommand _command;
        private readonly ILogger<PrintRequestFunction> _logger;

        public PrintRequestFunction(IPrintRequestCommand command, ILogger<PrintRequestFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("CertificatePrintRequest")]
        [QueueOutput(QueueNames.CertificatePrintStatusUpdate)]
        public async Task<List<CertificatePrintStatusUpdateMessage>> Run(
            [TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintRequestOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation("CertificatePrintRequest has started" + (myTimer.IsPastDue ? " later than scheduled" : string.Empty));

                var printStatusUpdateMessages = await _command.Execute();

                _logger.LogInformation("CertificatePrintRequest has finished");

                return printStatusUpdateMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CertificatePrintRequest has failed");
                return new List<CertificatePrintStatusUpdateMessage>();
            }
        }
    }
}
