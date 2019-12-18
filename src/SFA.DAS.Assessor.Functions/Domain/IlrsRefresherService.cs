using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public class IlrsRefresherService
    {
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assessorApiClient;

        public IlrsRefresherService(IDataCollectionServiceApiClient dataCollectionServiceApiClient, IAssessorServiceApiClient assessorServiceApiClient)
        {
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assessorApiClient = assessorServiceApiClient;
        }
    }
}
