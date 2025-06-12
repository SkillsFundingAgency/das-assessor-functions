using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    internal class OrganisationsDownloader : OfqualDownloader
    {
        public OrganisationsDownloader(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, IHttpClientFactory _httpClientFactory)
            :base(blobFileTransferClient, _httpClientFactory.CreateClient("Organisations"), OfqualDataType.Organisations)
        {
        }

        [FunctionName(nameof(DownloadOrganisationsData))]
        public async Task<string> DownloadOrganisationsData([ActivityTrigger] IDurableActivityContext unused, ILogger logger)
        {
            return await DownloadData(logger);
        }
    }
}
