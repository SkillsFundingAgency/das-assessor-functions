using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System;
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

        public async Task Execute(string message)
        {
            try
            {
                _logger.LogDebug($"PrintStatusUpdateCommand - Started for message {message}");

                var certificatePrintStatusUpdateMessage = JsonConvert.DeserializeObject<CertificatePrintStatusUpdateMessage>(message);
                await _certificateService.ProcessCertificatesPrintStatusUpdate(certificatePrintStatusUpdateMessage);
                
                _logger.LogDebug($"PrintStatusUpdateCommand - Completed for message {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PrintStatusUpdateCommand - Failed for message {message}");
                throw;
            }
        }
    }
}
