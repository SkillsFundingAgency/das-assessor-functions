using Microsoft.Azure.Functions.Worker;
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

        [Function(nameof(InsertQualificationsDataIntoStaging))]
        public async Task<int> InsertQualificationsDataIntoStaging([ActivityTrigger] IEnumerable<OfqualStandard> input, ILogger logger)
        {
            return await InsertDataIntoStagingTable(input, logger);
        }
    }
}
