using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        [FunctionName(nameof(InsertOrganisationsDataIntoStaging))]
        public async Task<int> InsertOrganisationsDataIntoStaging([ActivityTrigger] IDurableActivityContext context, ILogger logger)
        {
            var records = context.GetInput<IEnumerable<OfqualOrganisation>>();
            return await InsertDataIntoStagingTable(records, logger);
        }
    }
}
