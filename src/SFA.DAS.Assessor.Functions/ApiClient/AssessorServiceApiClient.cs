using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ApiClient
{
    public class AssessorServiceApiClient : IAssessorServiceApiClient
    {
        public HttpClient Client { get; }

        public AssessorServiceApiClient(HttpClient client, IOptions<AssessorApiAuthentication> assessorApiAuthenticationOptions, IConfiguration configuration)
        {
            var assessorBaseAddress = assessorApiAuthenticationOptions?.Value.ApiBaseAddress;
            client.BaseAddress = new Uri(assessorBaseAddress);
            
            var tokenService = new AssessorTokenService(assessorApiAuthenticationOptions.Value, configuration);
            var token = tokenService.GetToken();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client = client;
        }

        public async Task UpdateStandardSummary()
        {
            await Client.PostAsJsonAsync("api/v1/oppfinder/update-standard-summary", new { });
        }
    }
}
