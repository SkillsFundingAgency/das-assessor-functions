using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualFileMover
    {
        private readonly IOfqualDownloadsBlobFileTransferClient _blobFileTransferClient;
        private readonly ILogger<OfqualFileMover> _logger;

        public OfqualFileMover(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, ILogger<OfqualFileMover> logger)
        {
            _blobFileTransferClient = blobFileTransferClient;
            _logger = logger;
        }

        [Function(nameof(MoveOfqualFileToProcessed))]
        public async Task MoveOfqualFileToProcessed([ActivityTrigger] string filepath)
        {
            string filename = Path.GetFileName(filepath);

            _logger.LogInformation($"Moving {filename} to Processed folder.");

            var fileContents = await _blobFileTransferClient.DownloadFile($"Downloads/{filename}");
            await _blobFileTransferClient.UploadFile(fileContents, $"Processed/{filename}");
            await _blobFileTransferClient.DeleteFile($"Downloads/{filename}");

            _logger.LogInformation($"Moved {filename} to Processed folder.");
        }
    }
}