using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualFileMover
    {
        private readonly IOfqualDownloadsBlobFileTransferClient _blobFileTransferClient;

        public OfqualFileMover(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient)
        {
            _blobFileTransferClient = blobFileTransferClient;
        }

        [FunctionName(nameof(MoveOfqualFileToProcessed))]
        public async Task MoveOfqualFileToProcessed([ActivityTrigger] IDurableActivityContext context, ILogger logger)
        {
            string filepath = context.GetInput<string>();
            string filename = Path.GetFileName(filepath);

            logger.LogInformation($"Moving {filename} to Processed folder.");

            var fileContents = await _blobFileTransferClient.DownloadFile($"Downloads/{filename}");
            await _blobFileTransferClient.UploadFile(fileContents, $"Processed/{filename}");
            await _blobFileTransferClient.DeleteFile($"Downloads/{filename}");

            logger.LogInformation($"Moved {filename} to Processed folder.");
        }
    }
}