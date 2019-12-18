using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor
{
    public interface IAssessorServiceApiClient : IApiClientBase
    {
        Task UpdateStandardSummary();
        Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest request);
    }
}