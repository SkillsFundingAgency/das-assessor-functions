using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class QualificationsStager : OfqualStager
    {
        public QualificationsStager(IAssessorServiceRepository assessorServiceRepository)
        : base(assessorServiceRepository, OfqualDataType.Qualifications)
        {
        }

        [FunctionName(nameof(InsertQualificationsDataIntoStaging))]
        public async Task<int> InsertQualificationsDataIntoStaging([ActivityTrigger] IDurableActivityContext context, ILogger logger)
        {
            var input = context.GetInput<IEnumerable<OfqualStandard>>();
            return await InsertDataIntoStagingTable(input, logger);
        }
    }
}
