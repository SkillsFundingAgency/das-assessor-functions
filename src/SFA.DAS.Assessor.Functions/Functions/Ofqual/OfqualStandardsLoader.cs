using Microsoft.Azure.Functions.Worker;
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
        public async Task<int> LoadStandards([ActivityTrigger] string unused)
        {
            return await _assessorServiceRepository.LoadOfqualStandards();
        }
    }
}
