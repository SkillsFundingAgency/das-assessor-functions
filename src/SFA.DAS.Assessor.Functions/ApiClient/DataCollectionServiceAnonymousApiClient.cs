using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ApiClient
{
    public class DataCollectionServiceAnonymousApiClient : IDataCollectionServiceAnonymousApiClient
    {
        public string ApiVersion { get; }
        public HttpClient Client { get; }
        
        public DataCollectionServiceAnonymousApiClient(HttpClient client, IOptions<DataCollectionApiAuthentication> dataCollectionApiAuthenticationOptions)
        {
            ApiVersion = dataCollectionApiAuthenticationOptions.Value?.Version;

            client.BaseAddress = new Uri(dataCollectionApiAuthenticationOptions?.Value.ApiBaseAddress);
            Client = client;
        }

        public async Task<string> GetToken(DataCollectionTokenRequest request)
        {
            var response = await Client.PostAsJsonAsync($@"api/v{ApiVersion}/Token", request);
            var contents = await response.Content.ReadAsStringAsync();
            return contents;
        }
    }
}
