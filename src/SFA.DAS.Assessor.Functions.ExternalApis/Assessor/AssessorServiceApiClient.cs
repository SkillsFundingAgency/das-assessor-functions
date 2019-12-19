using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor
{
    public class AssessorServiceApiClient : ApiClientBase, IAssessorServiceApiClient
    {
        public AssessorServiceApiClient(HttpClient httpClient, IAssessorServiceTokenService tokenService, IOptions<AssessorApiAuthentication> options, ILogger<AssessorServiceApiClient> logger)
            : base (httpClient, tokenService, logger)
        {
            Client.BaseAddress = new Uri(options?.Value.ApiBaseAddress);
        }

        public async Task UpdateStandardSummary()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/oppfinder/update-standard-summary"))
            {
                await PostPutRequest(request);
            }
        }

        public async Task UpdateAssessorSetting(string settingName, string settingValue)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/assessorsetting/{settingName}/{settingValue}"))
            {
                //await PostPutRequest(request);
                await Task.FromResult(request);
            }
        }

        public async Task<string> GetAssessorSetting(string settingName)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/assessorsetting/{settingName}"))
            {
                //return await GetAsync<string>(request);
                return await Task.FromResult(new DateTime(2019, 9, 1).ToString());
            }
        }

        public async Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest importLearnerDetailRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/learnerdetails/import"))
            {
                return await PostPutRequestWithResponse<ImportLearnerDetailRequest, ImportLearnerDetailResponse>(request,
                    importLearnerDetailRequest);
            }
        }

        public string BaseAddress()
        {
            return Client.BaseAddress.ToString();
        }
    }
}
