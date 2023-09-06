using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualStandardsLoader
    {
        private readonly IAssessorServiceRepository _assessorServiceRepository;

        public OfqualStandardsLoader(IAssessorServiceRepository assessorServiceRepository)
        {
            _assessorServiceRepository = assessorServiceRepository;
        }

        [FunctionName(nameof(LoadStandards))]
        public async Task<int> LoadStandards([ActivityTrigger] IDurableActivityContext unused, ILogger logger)
        {
            return await _assessorServiceRepository.LoadOfqualStandards();
        }
    }
}
