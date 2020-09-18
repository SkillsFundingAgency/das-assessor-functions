using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;


namespace SFA.DAS.Assessor.Functions.Functions.ExternalApiDataSync
{
    public class RebuildExternalApiSandboxCommand: IRebuildExternalApiSandboxCommand
    {
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        public RebuildExternalApiSandboxCommand(IAssessorServiceApiClient assessorServiceApi)
        {
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Execute()
        {
            await _assessorServiceApi.RebuildExternalApiSandbox();
        }
    }
}