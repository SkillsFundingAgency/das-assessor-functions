using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
        private readonly ILogger<OuterApiClient> _logger;
        private readonly IOptions<Config.OuterApi> _config;
        private const string SubscriptionKeyRequestHeaderKey = "Ocp-Apim-Subscription-Key";
        private const string VersionRequestHeaderKey = "X-Version";

        public OuterApiClient(IOptions<Config.OuterApi> config, HttpClient httpClient, ILogger<OuterApiClient> logger)
        {
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {
            var requestUrl = new Uri(new Uri(_config.Value.BaseUrl), request.GetUrl);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            httpRequest.Headers.Add(SubscriptionKeyRequestHeaderKey, _config.Value.Key);
            httpRequest.Headers.Add(VersionRequestHeaderKey, "1");

            var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                _logger.LogWarning($"Page {requestUrl} cannot be found");
                return Activator.CreateInstance<TResponse>();
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }

            response.EnsureSuccessStatusCode();
            return default;
        }
    }
}