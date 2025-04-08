using CsvHelper.Configuration.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintStatusUpdateFunction
    {
        private readonly IPrintStatusUpdateCommand _command;
        private readonly ILogger<PrintStatusUpdateFunction> _logger;

        public PrintStatusUpdateFunction(IPrintStatusUpdateCommand command, ILogger<PrintStatusUpdateFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("CertificatePrintStatusUpdate")]
        [QueueOutput(QueueNames.CertificatePrintStatusUpdateErrors)]
        public async Task<List<CertificatePrintStatusUpdateErrorMessage>> Run(
            [QueueTrigger(QueueNames.CertificatePrintStatusUpdate)] CertificatePrintStatusUpdateMessage message)
        {
            try
            {
                _logger.LogDebug($"CertificatePrintStatusUpdate has started for {message.ToJson()}");

                var validationErrorMessages = await _command.Execute(message);

                if ((validationErrorMessages?.Count ?? 0) > 0)
                {
                    _logger.LogWarning($"CertificatePrintStatusUpdate has completed for {message.ToJson()} with {validationErrorMessages.Count} error(s)");
                }
                else
                {
                    _logger.LogDebug($"CertificatePrintStatusUpdate has completed for {message.ToJson()}");
                }
                return validationErrorMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CertificatePrintStatusUpdate has failed for {message.ToJson()}");
                throw;
            }
        }
    }
}
