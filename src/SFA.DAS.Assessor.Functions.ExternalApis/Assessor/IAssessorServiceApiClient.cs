using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor
{
    public interface IAssessorServiceApiClient : IApiClientBase
    {
        Task UpdateStandardSummary();
        Task SetAssessorSetting(string name, string value);
        Task<string> GetAssessorSetting(string name);
        Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest request);
    }
}