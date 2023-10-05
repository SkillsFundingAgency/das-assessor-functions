using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Types;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Ofs
{
    public class OfsRegisterApiClient : ApiClientBase, IOfsRegisterApiClient
    {
        public OfsRegisterApiClient(
            HttpClient httpClient,
            IOptions<OfsRegisterApiAuthentication> options,
            ILogger<OfsRegisterApiClient> logger)
            : base(httpClient, new Uri(options?.Value.ApiBaseAddress), logger)
        {
        }

        public async Task<List<OfsProvider>> GetProviders()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/provider"))
            {
                return await GetAsync<List<OfsProvider>>(request);
            }
        }
    }
}
