using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection
{
    public class DataCollectionServiceAnonymousApiClient : ApiClientBase, IDataCollectionServiceAnonymousApiClient
    {
        public string ApiVersion { get; }
        
        public DataCollectionServiceAnonymousApiClient(HttpClient httpClient, IOptions<DataCollectionApiAuthentication> options, ILogger<DataCollectionServiceAnonymousApiClient> logger)
            : base (httpClient, logger)
        {
            Client.BaseAddress = new Uri(options?.Value.ApiBaseAddress);
            ApiVersion = options.Value?.Version;
        }

        public async Task<string> GetToken(DataCollectionTokenRequest dataCollectionTokenRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $@"api/v{ApiVersion}/Token"))
            {
                return await PostPutRequestWithResponse(request, dataCollectionTokenRequest);
            }
        }
    }
}
