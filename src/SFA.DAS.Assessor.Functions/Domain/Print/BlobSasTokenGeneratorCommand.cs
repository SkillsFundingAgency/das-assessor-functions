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
                if(!_options.Enabled)
                {
                    _logger.LogInformation($"BlobSasTokenGeneratorCommand cannot be started, it is not enabled");
                    return;
                }

                _logger.LogInformation($"BlobSasTokenGeneratorCommand started");

                if(string.IsNullOrEmpty(_options.SasIPAddress))
                {
                    _logger.LogError("BlobSasTokenGeneratorCommand failed - the IP address restriction for a SAS token must be specified");
                    return;
                }

                var sasToken = _blobFileTransferClient.GetContainerSasUri(
                    _options.SasGroupPolicyIdentifier, 
                    DateTime.UtcNow, 
                    DateTime.UtcNow.AddDays(_options.SasExpiryDays), 
                    _options.SasIPAddress);

                _logger.LogInformation($"BlobSasTokenGeneratorCommand - generated '{_options.SasGroupPolicyIdentifier}' Sas token on '{_blobFileTransferClient.ContainerName}' restricted to '{_options.SasIPAddress}' for {_options.SasExpiryDays} days");

                var secureMessage = await _secureMessageServiceApiClient.CreateMessage(sasToken, _options.SecureMessageTtl);

                _logger.LogInformation($"BlobSasTokenGeneratorCommand - generated secure message for Sas token");

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
