using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection
{
    public class DataCollectionServiceApiClient : ApiClientBase, IDataCollectionServiceApiClient
    {
        public string ApiVersion { get; }
        
        public DataCollectionServiceApiClient(HttpClient client, IDataCollectionTokenService tokenService, IOptions<DataCollectionApiAuthentication> options, ILogger<DataCollectionServiceApiClient> logger)
            : base(client, tokenService, logger)
        {
            Client.BaseAddress = new Uri(options?.Value.ApiBaseAddress);
            ApiVersion = options.Value?.Version;
        }

        public async Task<List<string>> GetAcademicYears(DateTime dateTimeUtc)
        {
            // THIS DC API DOES NOT RETURN THE CORRECT RESULT - THE DC TEAM IS WORKING ON IT

            /*var requestUri = $@"/api/v{ApiVersion}/ilr-data/academic-years?" +
                $"dateTimeUtc={WebUtility.UrlEncode(dateTimeUtc.ToString("o"))}";

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var response = await GetAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<string>>(json, JsonSettings));
                }

                return default;
            }*/

            // HARDCODED TO CONTINUE DEVELOPMENT
            return await Task.FromResult(new List<string> { "1920" });
        }

        public async Task<DataCollectionProvidersPage> GetProviders(string source, DateTime startDateTime, int? pageSize = null, int? pageNumber = null)
        {
            var requestUri = $@"/api/v{ApiVersion}/ilr-data/{source}/providers?" +
                $"startDateTime={WebUtility.UrlEncode(startDateTime.ToString("o"))}" +
                (pageSize != null 
                    ? $"&pageSize={pageSize}" 
                    : string.Empty) +
                (pageNumber != null 
                    ? $"&pageNumber={pageNumber}" 
                    : string.Empty);

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var response = await GetAsync(request);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var providers = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<int>>(json, JsonSettings));
                    var pagingInfo = response.Headers.GetValues("X-Pagination")?.FirstOrDefault();
                    
                    if (providers.Count > 0 && !string.IsNullOrEmpty(pagingInfo))
                    {
                        return new DataCollectionProvidersPage
                        {
                            Providers = providers,
                            PagingInfo = JsonConvert.DeserializeObject<DataCollectionPagingInfo>(pagingInfo)
                        };
                    }
                }
                else if(response?.StatusCode == HttpStatusCode.NoContent)
                {
                    return new DataCollectionProvidersPage();
                }

                return null;
            }
        }

        public async Task<DataCollectionLearnersPage> GetLearners(string source, DateTime startDateTime, int? aimType = null, int? standardCode = null, List<int> fundModels = null, int? pageSize = null, int? pageNumber = null)
        {
            var requestUri = $@"/api/v{ApiVersion}/ilr-data/{source}/learners?" +
                $"startDateTime={WebUtility.UrlEncode(startDateTime.ToString("o"))}";

            return await GetLearnersInternal(requestUri, aimType, standardCode, fundModels, pageSize, pageNumber);
        }

        public async Task<DataCollectionLearnersPage> GetLearners(string source, int ukprn, int? aimType = null, int? standardCode = null, List<int> fundModels = null, int? pageSize = null, int? pageNumber = null)
        {
            var requestUri = $@"/api/v{ApiVersion}/ilr-data/{source}/learners?" +
                $"ukprn={ukprn}";

            return await GetLearnersInternal(requestUri, aimType, standardCode, fundModels, pageSize, pageNumber);
        }

        private async Task<DataCollectionLearnersPage> GetLearnersInternal(string learnersRequestUri, int? aimType = null, int? standardCode = null, List<int> fundModels = null, int? pageSize = null, int? pageNumber = null)
        {
            var requestUri = learnersRequestUri
                + (aimType != null
                    ? $"&aimType={aimType.Value}"
                    : string.Empty)
                + (standardCode != null
                    ? $"&standardCode={standardCode.Value}"
                    : string.Empty)
                + (fundModels != null
                    ? string.Join(string.Empty, fundModels.ConvertAll(p => $"&fundModel={p}").ToArray())
                    : string.Empty)
                + (pageSize != null
                    ? $"&pageSize={pageSize.Value}"
                    : string.Empty)
                + (pageNumber != null
                    ? $"&pageNumber={pageNumber.Value}"
                    : string.Empty);

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var response = await GetAsync(request);

                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var learners = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<DataCollectionLearner>>(json, JsonSettings));
                    var pagingInfo = response.Headers.GetValues("X-Pagination")?.FirstOrDefault();

                    if (learners.Count > 0 && !string.IsNullOrEmpty(pagingInfo))
                    {
                        return new DataCollectionLearnersPage
                        {
                            Learners = learners,
                            PagingInfo = JsonConvert.DeserializeObject<DataCollectionPagingInfo>(pagingInfo)
                        };
                    }
                }
                else if (response?.StatusCode == HttpStatusCode.NoContent)
                {
                    return new DataCollectionLearnersPage();
                }
            }

            return null;
        }

        public string BaseAddress()
        {
            return Client.BaseAddress.ToString();
        }
    }
}
