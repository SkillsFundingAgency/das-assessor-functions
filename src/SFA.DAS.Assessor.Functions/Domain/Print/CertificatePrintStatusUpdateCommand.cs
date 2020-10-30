using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class CertificatePrintStatusUpdateCommand : ICertificatePrintStatusUpdateCommand
    {
        private readonly ILogger<CertificatePrintStatusUpdateCommand> _logger;
        private readonly ICertificateService _certificateService;
        
        public CertificatePrintStatusUpdateCommand(ILogger<CertificatePrintStatusUpdateCommand> logger, ICertificateService certificateService)
        {
            _logger = logger;
            _certificateService = certificateService;
        }

        public async Task Execute(string message)
        {
            try
            {
                _logger.LogDebug($"CertificatePrintStatusUpdateCommand started for message {message}");

                var certificatePrintStatusUpdateMessage = JsonConvert.DeserializeObject<CertificatePrintStatusUpdateMessage>(message);
                await _certificateService.ProcessCertificatesPrintStatusUpdates(certificatePrintStatusUpdateMessage.CertificatePrintStatusUpdates);
                
                _logger.LogDebug($"CertificatePrintStatusUpdateCommand completed for message {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CertificatePrintStatusUpdateCommand failed for message {message}");
                throw;
            }
        }
    }
}
