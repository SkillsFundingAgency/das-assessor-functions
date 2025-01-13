using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OrganisationsStager : OfqualStager
    {
        public OrganisationsStager(IAssessorServiceRepository assessorServiceRepository) 
            : base(assessorServiceRepository, OfqualDataType.Organisations)
        {
        }

        [Function(nameof(InsertOrganisationsDataIntoStaging))]
        public async Task<int> InsertOrganisationsDataIntoStaging([ActivityTrigger] IEnumerable<OfqualOrganisation> records, ILogger logger)
        {
            return await InsertDataIntoStagingTable(records, logger);
        }
    }
}
