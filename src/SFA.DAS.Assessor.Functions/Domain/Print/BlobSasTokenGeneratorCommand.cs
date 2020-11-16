using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class BlobSasTokenGeneratorCommand : IBlobSasTokenGeneratorCommand
    {
        private readonly ILogger<BlobSasTokenGeneratorCommand> _logger;
        private readonly INotificationService _notificationService;
        private readonly IExternalBlobFileTransferClient _blobFileTransferClient;
        private readonly ISecureMessageServiceApiClient _secureMessageServiceApiClient;
        private readonly BlobSasTokenGeneratorOptions _options;

        public BlobSasTokenGeneratorCommand(
            ILogger<BlobSasTokenGeneratorCommand> logger,
            INotificationService notificationService,
            IExternalBlobFileTransferClient blobFileTransferClient,
            ISecureMessageServiceApiClient secureMessageServiceApiClient,
            IOptions<BlobSasTokenGeneratorOptions> SasTokenGeneratorOptions)
        {
            _logger = logger;
            _notificationService = notificationService;
            _blobFileTransferClient = blobFileTransferClient;
            _secureMessageServiceApiClient = secureMessageServiceApiClient;
            _options = SasTokenGeneratorOptions?.Value;
        }

        public async Task Execute()
        {
            try
            {
                var sasToken = _blobFileTransferClient.GetContainerSasUri(_options.SasIPAddress, DateTime.UtcNow.AddDays(_options.SasExpiryDays));
                
                var secureMessage = await _secureMessageServiceApiClient.CreateMessage(sasToken, _options.SecureMessageTtl);
                
                await _notificationService.SendSasToken(secureMessage.Links.Web);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BlobSasTokenGeneratorCommand failed");
                throw;
            }
        }
    }
}
