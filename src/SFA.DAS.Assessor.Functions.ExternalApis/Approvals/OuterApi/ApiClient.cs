using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
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

            try
            {
                var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Resource not found: {requestUrl}");
                    return default;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError($"Unauthorized access: {requestUrl}");
                    throw new UnauthorizedAccessException("Unauthorized access to API.");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogError($"Forbidden access: {requestUrl}");
                    throw new AccessViolationException("Forbidden access to API.");
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError($"Rate limit exceeded: {requestUrl}");

                    throw new HttpRequestException("Rate limit exceeded.");
                }
                else if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    _logger.LogError($"API request failed: {requestUrl} - Status Code: {response.StatusCode} - Content: {errorContent}");
                    throw new HttpRequestException($"API request failed with status code {response.StatusCode}.  Details: {errorContent}");
                }

                var json = string.Empty;
                try
                {
                    json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResponse>(json);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, $"Error deserializing JSON: {requestUrl} - JSON: {json}");
                    throw new JsonReaderException("Error deserializing API response.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deserializing JSON: {requestUrl}");
                    throw new Exception("Error during deserialization of API response", ex);
                }

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP request error: {requestUrl}");
                throw new HttpRequestException($"Error making API request to {requestUrl}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in Get request: {requestUrl}");
                throw;
            }
        }
    }
}