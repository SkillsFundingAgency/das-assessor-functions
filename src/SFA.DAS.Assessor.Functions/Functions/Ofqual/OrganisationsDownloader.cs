using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    internal class OrganisationsDownloader : OfqualDownloader
    {
        public OrganisationsDownloader(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, IHttpClientFactory _httpClientFactory, ILogger<OrganisationsDownloader> logger)
            :base(blobFileTransferClient, _httpClientFactory.CreateClient("Organisations"), OfqualDataType.Organisations, logger)
        {
        }

        [Function(nameof(DownloadOrganisationsData))]
        public async Task<string> DownloadOrganisationsData([ActivityTrigger] Task unused)
        {
            return await DownloadData();
        }
    }
}
