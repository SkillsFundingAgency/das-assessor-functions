using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ApiClient
{
    public class DataCollectionServiceApiClient : IDataCollectionServiceApiClient
    {
        public string ApiVersion { get; }
        public HttpClient Client { get; }

        public DataCollectionServiceApiClient(HttpClient client, IDataCollectionTokenService dataCollectionTokenService, IOptions<DataCollectionApiAuthentication> dataCollectionApiAuthenticationOptions)
        {
            ApiVersion = dataCollectionApiAuthenticationOptions.Value?.Version;

            client.BaseAddress = new Uri(dataCollectionApiAuthenticationOptions?.Value.ApiBaseAddress);
            
            var token = dataCollectionTokenService.GetToken();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client = client;
        }

        public async Task<List<int>> GetProviders(DateTime startDateTime)
        {
            var response = await Client.GetAsync($@"/api/v{ApiVersion}/ilr-data/1920/providers?startDateTime={startDateTime.ToString("o")}");

            // when there are multiple pages these would need to be paginated
            var paginationHeader = response.Headers.GetValues("X-Pagination");

            var contents = await response.Content.ReadAsStringAsync();

            var ukprn = JsonConvert.DeserializeObject<List<int>>(contents);

            return ukprn;
        }

        public async Task<List<DataCollectionLearner>> GetLearners(int ukprn)
        {
            var response = await Client.GetAsync($@"/api/v{ApiVersion}/ilr-data/1920/learners?ukprn={ukprn}");

            // when there are multiple pages these would need to be paginated
            var paginationHeader = response.Headers.GetValues("X-Pagination");

            var contents = await response.Content.ReadAsStringAsync();

            var learners = JsonConvert.DeserializeObject<List<DataCollectionLearner>>(contents);

            return learners;
        }
    }
}
