using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    internal class QualificationsDownloader : OfqualDownloader
    {
        public QualificationsDownloader(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, IHttpClientFactory _httpClientFactory)
            :base(blobFileTransferClient, _httpClientFactory.CreateClient("Qualifications"), OfqualDataType.Qualifications)
        {
        }

        [FunctionName(nameof(DownloadQualificationsData))]
        public async Task<string> DownloadQualificationsData([ActivityTrigger] IDurableActivityContext unused, ILogger logger)
        {
            return await DownloadData(logger);
        }
    }
}
