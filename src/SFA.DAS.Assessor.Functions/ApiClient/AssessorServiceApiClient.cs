using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ApiClient
{
    public class AssessorServiceApiClient : IAssessorServiceApiClient
    {
        private readonly AssessorHttpClient _httpClient;

        public AssessorServiceApiClient(AssessorHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdateStandardSummary()
        {
            await _httpClient.PostAsJsonAsync("api/v1/oppfinder/update-standard-summary", new { });
        }
    }
}
