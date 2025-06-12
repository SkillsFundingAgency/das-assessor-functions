using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintStatusUpdateCommand : IPrintStatusUpdateCommand
    {
        private readonly ILogger<PrintStatusUpdateCommand> _logger;
        private readonly ICertificateService _certificateService;
        
        public PrintStatusUpdateCommand(ILogger<PrintStatusUpdateCommand> logger, ICertificateService certificateService)
        {
            _logger = logger;
            _certificateService = certificateService;
        }

        public async Task<List<CertificatePrintStatusUpdateErrorMessage>> Execute(CertificatePrintStatusUpdateMessage message)
        {
            var validationErrorMessages = new List<CertificatePrintStatusUpdateErrorMessage>();

            try
            {
                var validationResponse = await _certificateService.ProcessCertificatesPrintStatusUpdate(message);
                if(validationResponse.Errors.Any())
                {
                    var errorMessages = validationResponse.Errors.
                        Where(p => p.ValidationStatusCode != ValidationStatusCode.Warning).
                        Select(s => s.ErrorMessage);

                    if (errorMessages.Any())
                    {
                        validationErrorMessages.Add(new CertificatePrintStatusUpdateErrorMessage
                        {
                            CertificatePrintStatusUpdate = message,
                            ErrorMessages = errorMessages.ToList()
                        });
                    }

                    var warningMessages = validationResponse.Errors.
                            Where(p => p.ValidationStatusCode == ValidationStatusCode.Warning).
                            Select(s => s.ErrorMessage);

                    foreach (var warningMessage in warningMessages)
                    {
                        _logger.LogWarning($"PrintStatusUpdateCommand - Processed message {message.ToJson()} with warning: {warningMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PrintStatusUpdateCommand - Failed for message {message.ToJson()}");
                throw;
            }

            return validationErrorMessages;
        }
    }
}
