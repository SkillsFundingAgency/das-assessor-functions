using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;


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
            var dataSyncRequest = new RebuildExternalApiSandboxRequest();
            
            await _assessorServiceApi.RebuildExternalApiSandbox(dataSyncRequest);
        }
    }
}