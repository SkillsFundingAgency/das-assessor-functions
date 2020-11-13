using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class BlobSasTokenGeneratorCommand : IBlobSasTokenGeneratorCommand
    {
        private readonly ILogger<BlobSasTokenGeneratorCommand> _logger;
        private readonly IBlobFileTransferClient _blobFileTransferClient;

        public BlobSasTokenGeneratorCommand(
            ILogger<BlobSasTokenGeneratorCommand> logger,
            IBlobFileTransferClient blobFileTransferClient)
        {
            _logger = logger;
            
            _blobFileTransferClient = blobFileTransferClient;
            _blobFileTransferClient.ContainerName = "";
        }

        public async Task Execute()
        {
            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BlobSasTokenGeneratorCommand failed");
                throw;
            }
        }
    }
}
