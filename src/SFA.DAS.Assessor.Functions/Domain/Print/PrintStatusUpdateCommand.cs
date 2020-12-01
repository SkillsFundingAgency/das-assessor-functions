using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<string>> Execute(string message)
        {
            var validationErrorMessages = new List<string>();

            try
            {
                _logger.LogDebug($"PrintStatusUpdateCommand - Started for message {message}");

                var certificatePrintStatusUpdateMessage = JsonConvert.DeserializeObject<CertificatePrintStatusUpdateMessage>(message);
                
                var validationResponse = await _certificateService.ProcessCertificatesPrintStatusUpdate(certificatePrintStatusUpdateMessage);
                if(validationResponse.Errors.Any(p => p.ValidationStatusCode != ValidationStatusCode.Warning))
                {
                    var certificatePrintStatusUpdateErrorMessage = new CertificatePrintStatusUpdateErrorMessage
                    {
                        CertificatePrintStatusUpdate = certificatePrintStatusUpdateMessage,
                        ErrorMessages = validationResponse.Errors.
                            Where(p => p.ValidationStatusCode != ValidationStatusCode.Warning).
                            Select(s => s.ErrorMessage).
                            ToList()
                    };

                    validationErrorMessages.Add(JsonConvert.SerializeObject(certificatePrintStatusUpdateErrorMessage));

                    _logger.LogInformation($"PrintStatusUpdateCommand - Completed for message {message} with {certificatePrintStatusUpdateErrorMessage.ErrorMessages.Count} error(s)");
                }
                else
                {
                    _logger.LogDebug ($"PrintStatusUpdateCommand - Completed for message {message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PrintStatusUpdateCommand - Failed for message {message}");
                throw;
            }

            return validationErrorMessages;
        }
    }
}
