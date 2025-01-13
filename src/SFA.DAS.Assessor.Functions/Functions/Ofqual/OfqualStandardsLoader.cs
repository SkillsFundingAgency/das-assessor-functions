using Microsoft.Azure.Functions.Worker;
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

        [Function(nameof(LoadStandards))]
        public async Task<int> LoadStandards([ActivityTrigger] string unused, ILogger logger)
        {
            return await _assessorServiceRepository.LoadOfqualStandards();
        }
    }
}
