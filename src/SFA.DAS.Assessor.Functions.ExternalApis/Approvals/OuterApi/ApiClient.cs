using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi.Config;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi
{
    public interface IOuterApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
    }

    public class OuterApiClient : IOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<Config.OuterApi> _config;
        const string SubscriptionKeyRequestHeaderKey = "Ocp-Apim-Subscription-Key";
        const string VersionRequestHeaderKey = "X-Version";

        public OuterApiClient (HttpClient httpClient, IOptions<Config.OuterApi>  config)
        {
            _httpClient = httpClient;
            _config = config;
            _httpClient.BaseAddress = new Uri(_config.Value.BaseUrl);
        }
        
        public async Task<TResponse> Get<TResponse>(IGetApiRequest request) 
        {
            AddHeaders();

            var response = await _httpClient.GetAsync(request.GetUrl).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                return default;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }

            response.EnsureSuccessStatusCode();
            return default;
        }

        private void AddHeaders()
        {
            //The http handler life time is set to 5 minutes
            //hence once the headers are added they don't need added again
            if (_httpClient.DefaultRequestHeaders.Contains(SubscriptionKeyRequestHeaderKey)) return;

            _httpClient.DefaultRequestHeaders.Add(SubscriptionKeyRequestHeaderKey, _config.Value.Key);
            _httpClient.DefaultRequestHeaders.Add(VersionRequestHeaderKey, "1");
        }
    }

   
}