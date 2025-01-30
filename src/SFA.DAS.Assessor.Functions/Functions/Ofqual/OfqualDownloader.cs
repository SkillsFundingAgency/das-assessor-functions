using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    internal abstract class OfqualDownloader
    {
        private readonly IOfqualDownloadsBlobFileTransferClient _blobFileTransferClient;
        private readonly HttpClient _httpClient;
        private readonly OfqualDataType _ofqualFileType;
        private readonly ILogger<OfqualDownloader> _logger;

        protected OfqualDownloader(
            IOfqualDownloadsBlobFileTransferClient blobFileTransferClient,
            HttpClient httpClient,
            OfqualDataType ofqualFileType,
            ILogger<OfqualDownloader> logger)
        {
            _blobFileTransferClient = blobFileTransferClient;
            _httpClient = httpClient;
            _ofqualFileType = ofqualFileType;
            _logger = logger;
        }

        protected async Task<string> DownloadData()
        {
            _logger.LogInformation($"Downloading {_ofqualFileType} data from {_httpClient.BaseAddress}.");

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(string.Empty);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Finished downloading {_ofqualFileType} data.");

                return await SaveDownloadedFile(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error: {ex.Message}");
                throw;
            }
        }

        private async Task<string> SaveDownloadedFile(string content)
        {
            string filename = OfqualDataFileName.CreateForFileType(_ofqualFileType);
            string filePath = $"Downloads/{filename}";

            await _blobFileTransferClient.UploadFile(content, filePath);
            _logger.LogInformation($"File saved at {filePath}");

            return filePath;
        }
    }
}

