using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    internal class QualificationsDownloader : OfqualDownloader
    {
        public QualificationsDownloader(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, IHttpClientFactory _httpClientFactory, ILogger<QualificationsDownloader> logger)
            :base(blobFileTransferClient, _httpClientFactory.CreateClient("Qualifications"), OfqualDataType.Qualifications, logger)
        {
        }

        [Function(nameof(DownloadQualificationsData))]
        public async Task<string> DownloadQualificationsData([ActivityTrigger] string unused)
        {
            return await DownloadData();
        }
    }
}
