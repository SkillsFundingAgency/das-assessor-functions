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
        
        public DataCollectionServiceApiClient(HttpClient httpClient, IDataCollectionTokenService tokenService, IOptions<DataCollectionApiAuthentication> options, ILogger<DataCollectionServiceApiClient> logger)
            : base(httpClient, new Uri(options?.Value.ApiBaseAddress), logger)
        {
            ApiVersion = options.Value?.Version;
        }

        public async Task<List<string>> GetAcademicYears(DateTime dateTimeUtc)
        {
            var requestUri = $@"/api/v{ApiVersion}/academic-years?" +
                $"dateTimeUtc={WebUtility.UrlEncode(dateTimeUtc.ToString("o"))}";

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var response = await GetAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var sources = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<string>>(json, JsonSettings));
                    
                    sources.Sort();
                    return sources;
                }

                return default;
            }
        }

        public async Task<DataCollectionProvidersPage> GetProviders(string source, DateTime startDateTime, int? pageSize, int? pageNumber)
        {
            var requestUri = $@"/api/v{ApiVersion}/ilr-data/providers/{source}?" +
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

        public async Task<DataCollectionLearnersPage> GetLearners(string source, DateTime startDateTime, int? aimType, int? standardCode, List<int> fundModels, int? progType, int? pageSize, int? pageNumber)
        {
            var requestUri = $@"/api/v{ApiVersion}/ilr-data/learners/{source}?" +
                $"startDateTime={WebUtility.UrlEncode(startDateTime.ToString("o"))}";

            return await GetLearnersInternal(requestUri, aimType, standardCode, fundModels, progType, pageSize, pageNumber);
        }

        public async Task<DataCollectionLearnersPage> GetLearners(string source, int ukprn, int? aimType, int? standardCode, List<int> fundModels, int? progType, int? pageSize, int? pageNumber)
        {
            var requestUri = $@"/api/v{ApiVersion}/ilr-data/learners/{source}?" +
                $"ukprn={ukprn}";

            return await GetLearnersInternal(requestUri, aimType, standardCode, fundModels, progType, pageSize, pageNumber);
        }

        private async Task<DataCollectionLearnersPage> GetLearnersInternal(string learnersRequestUri, int? aimType = null, int? standardCode = null, List<int> fundModels = null, int? progType = null, int? pageSize = null, int? pageNumber = null)
        {
            var requestUri = learnersRequestUri
                + (aimType != null
                    ? $"&aimType={aimType.Value}"
                    : string.Empty)
                + (standardCode != null
                    ? $"&standardCode={standardCode.Value}"
                    : string.Empty)
                + (fundModels != null
                    ? string.Join(string.Empty, fundModels.ConvertAll(p => $"&fundModel={p}"))
                    : string.Empty)
                + (progType != null
                    ? $"&progType={progType.Value}"
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
    }
}
