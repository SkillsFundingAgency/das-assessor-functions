using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.Assessor.Functions.ExternalApis.Exceptions;

namespace SFA.DAS.Assessor.Functions.ExternalApis
{
    public abstract class ApiClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClientBase> _logger;
        private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;

        protected readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        protected ApiClientBase(HttpClient httpClient, Uri baseAddress, ILogger<ApiClientBase> logger)
        {
            if(string.IsNullOrEmpty(baseAddress.AbsolutePath))
            {
                throw new Exception("Must specify base address");
            }
            
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.BaseAddress = baseAddress;
            
            _logger = logger;
            
            _retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public string BaseAddress()
        {
            return _httpClient.BaseAddress.ToString();
        }

        private static void RaiseResponseError(string message, string failedRequestUri, HttpResponseMessage failedResponse)
        {
            if (failedResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new EntityNotFoundException(message, CreateRequestException(failedRequestUri, failedResponse));
            }

            throw CreateRequestException(failedRequestUri, failedResponse);
        }

        private static void RaiseResponseError(string failedRequestUri, HttpResponseMessage failedResponse)
        {
            throw CreateRequestException(failedRequestUri, failedResponse);
        }

        private static HttpRequestException CreateRequestException(string failedRequestUri, HttpResponseMessage failedResponse)
        {
            return new HttpRequestException(
                string.Format($"The Client request for {{0}} failed. Response Status: {{1}}, Response Body: {{2}}",
                    failedRequestUri,
                    (int)failedResponse.StatusCode,
                    failedResponse.Content.ReadAsStringAsync().Result));
        }

        protected async Task<T> GetAsync<T>(HttpRequestMessage requestMessage, string message = null)
        {
            var response = await GetAsync(requestMessage, message);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(json, JsonSettings));
            }

            return default;
        }

        protected async Task<HttpResponseMessage> GetAsync(HttpRequestMessage requestMessage, string message = null)
        {
            if (requestMessage.Method != HttpMethod.Get)
            {
                throw new ArgumentOutOfRangeException(nameof(requestMessage), $"Request must be {nameof(HttpMethod.Get)}");
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                if (requestMessage.Method == HttpMethod.Get)
                {
                    return await _httpClient.GetAsync(requestMessage.RequestUri);
                }

                return null;
            });

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                if (message == null)
                {
                    if (!requestMessage.RequestUri.IsAbsoluteUri)
                        message = "Could not find " + requestMessage.RequestUri;
                    else
                        message = "Could not find " + requestMessage.RequestUri.PathAndQuery;
                }

                RaiseResponseError(message, requestMessage.RequestUri.ToString(), response);
            }

            return response;
        }

        protected async Task<string> PostPutRequestWithResponse<T>(HttpRequestMessage requestMessage, T model)
        {
            var response = await PostPutRequestWithResponseInternal(requestMessage, model);
            return await response?.Content.ReadAsStringAsync();
        }

        protected async Task<U> PostPutRequestWithResponse<T, U>(HttpRequestMessage requestMessage, T model)
        {
            var response = await PostPutRequestWithResponseInternal(requestMessage, model);
            var json = await response?.Content.ReadAsStringAsync();

            if (response?.StatusCode == HttpStatusCode.OK
                || response?.StatusCode == HttpStatusCode.Created
                || response?.StatusCode == HttpStatusCode.NoContent)
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<U>(json, JsonSettings));
            }
            else
            {
                _logger.LogInformation($"HttpRequestException: Status Code: {response?.StatusCode} Body: {json}");
                throw new HttpRequestException(json);
            }
        }

        private async Task<HttpResponseMessage> PostPutRequestWithResponseInternal<T>(HttpRequestMessage requestMessage, T model)
        {
            if (requestMessage.Method != HttpMethod.Post && requestMessage.Method != HttpMethod.Put)
            {
                throw new ArgumentOutOfRangeException(nameof(requestMessage), $"Request must be either {nameof(HttpMethod.Post)} or {nameof(HttpMethod.Put)}");
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                if (requestMessage.Method == HttpMethod.Post)
                {
                    return await _httpClient.PostAsJsonAsync(requestMessage.RequestUri, model);
                }
                else if (requestMessage.Method == HttpMethod.Put)
                {
                    return await _httpClient.PutAsJsonAsync(requestMessage.RequestUri, model);
                }

                return null;
            });

            return response;
        }

        protected async Task PostPutRequest<T>(HttpRequestMessage requestMessage, T model)
        {
            if (requestMessage.Method != HttpMethod.Post && requestMessage.Method != HttpMethod.Put)
            {
                throw new ArgumentOutOfRangeException(nameof(requestMessage), $"Request must be either {nameof(HttpMethod.Post)} or {nameof(HttpMethod.Put)}");
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                if (requestMessage.Method == HttpMethod.Post)
                {
                    return await _httpClient.PostAsJsonAsync(requestMessage.RequestUri, model);
                }
                else if (requestMessage.Method == HttpMethod.Put)
                {
                    return await _httpClient.PutAsJsonAsync(requestMessage.RequestUri, model);
                }

                return null;
            });

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException();
            }
        }

        protected async Task PostPutRequest(HttpRequestMessage requestMessage)
        {
            if (requestMessage.Method != HttpMethod.Post && requestMessage.Method != HttpMethod.Put)
            {
                throw new ArgumentOutOfRangeException(nameof(requestMessage), $"Request must be either {nameof(HttpMethod.Post)} or {nameof(HttpMethod.Put)}");
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                if (requestMessage.Method == HttpMethod.Post)
                {
                    return await _httpClient.PostAsync(requestMessage.RequestUri, null);
                }
                else if (requestMessage.Method == HttpMethod.Put)
                {
                    return await _httpClient.PutAsync(requestMessage.RequestUri, null);
                }
                
                return null;
            });

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException();
            }
        }

        protected async Task Delete(HttpRequestMessage requestMessage)
        {
            if (requestMessage.Method != HttpMethod.Delete)
            {
                throw new ArgumentOutOfRangeException(nameof(requestMessage), $"Request must be {nameof(HttpMethod.Delete)}");
            }
            
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                if (requestMessage.Method == HttpMethod.Delete)
                {
                    return await _httpClient.DeleteAsync(requestMessage.RequestUri);
                }
                
                return null;
            });

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException();
            }
        }
    }
}
