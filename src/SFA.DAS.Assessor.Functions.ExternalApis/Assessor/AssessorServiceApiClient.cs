using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ApiClient.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor
{
    public class AssessorServiceApiClient : ApiClientBase, IAssessorServiceApiClient
    {
        public AssessorServiceApiClient(
            HttpClient httpClient, 
            IAssessorServiceTokenService tokenService, 
            IOptions<AssessorApiAuthentication> options, 
            ILogger<AssessorServiceApiClient> logger)
            : base(httpClient, tokenService, logger)
        {
            Client.BaseAddress = new Uri(options?.Value.ApiBaseAddress);
        }

        public async Task UpdateStandardSummary()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/oppfinder/update-standard-summary"))
            {
                await PostPutRequest(request);
            }
        }

        public async Task SetAssessorSetting(string name, string value)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/assessor-setting/{name}/{value}"))
            {
                await PostPutRequest(request);
            }
        }

        public async Task<string> GetAssessorSetting(string name)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/assessor-setting/{name}"))
            {
                return await GetAsync<string>(request);
            }
        }

        public async Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest importLearnerDetailRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/learnerdetails/import"))
            {
                return await PostPutRequestWithResponse<ImportLearnerDetailRequest, ImportLearnerDetailResponse>(request,
                    importLearnerDetailRequest);
            }
        }

        public string BaseAddress()
        {
            return Client.BaseAddress.ToString();
        }

        public async Task<BatchLogResponse> CreateBatchLog(CreateBatchLogRequest createBatchLogRequest)
        {
            var responseMessage = await Client.PostAsJsonAsync($"/api/v1/batches", createBatchLogRequest);
            return await responseMessage.Content.ReadAsAsync<BatchLogResponse>();
        }

        public async Task ChangeStatusToPrinted(int batchNumber, IEnumerable<CertificateResponse> responses)
        {
            // the certificate printed status be will updated in chunks to stay within the WAF message size limits
            const int chunkSize = 100;

            var certificateStatuses = responses.Select(
                q => new CertificateStatus
                {
                    CertificateReference = q.CertificateReference,
                    Status = Assessor.Constants.CertificateStatus.Printed
                }).ToList();

            foreach (var certificateStatusesChunk in certificateStatuses.ChunkBy(chunkSize))
            {
                var updateCertificatesBatchToIndicatePrintedRequest = new UpdateCertificatesBatchToIndicatePrintedRequest
                {
                    BatchNumber = batchNumber,
                    CertificateStatuses = certificateStatusesChunk
                };

                await Client.PutAsJsonAsync($"/api/v1/certificates/{batchNumber}", updateCertificatesBatchToIndicatePrintedRequest);
            }
        }

        public async Task CompleteSchedule(Guid scheduleRunId)
        {
            await Client.PostAsync($"/api/v1/schedule?scheduleRunId={scheduleRunId}", null);
        }

        public async Task<IEnumerable<CertificateResponse>> GetCertificatesToBePrinted()
        {
            var response = await Client.GetAsync("/api/v1/certificates/tobeprinted");

            var certificates = await response.Content.ReadAsAsync<List<CertificateResponse>>();
            if (response.IsSuccessStatusCode)
            {
                Logger.Log(LogLevel.Information, $"Getting Certificates to be printed - Status code returned: {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
            }
            else
            {
                Logger.Log(LogLevel.Information, $"Getting Certificates to be printed - Status code returned: {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
            }

            return certificates;
        }

        public async Task<BatchLogResponse> GetCurrentBatchLog()
        {
            var response = await Client.GetAsync("/api/v1/batches/latest");

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new BatchLogResponse
                {
                    BatchNumber = 0
                };
            }

            return await response.Content.ReadAsAsync<BatchLogResponse>();
        }

        public async Task<BatchLogResponse> GetGetBatchLogByBatchNumber(string batchNumber)
        {
            var response = await Client.GetAsync($"/api/v1/batches/{batchNumber}");

            return await response.Content.ReadAsAsync<BatchLogResponse>();
        }

        public async Task<ScheduleRun> GetSchedule(ScheduleType scheduleType)
        {
            var response = await Client.GetAsync($"/api/v1/schedule/runnow?scheduleType={(int)scheduleType}");

            if (!response.IsSuccessStatusCode) return null;

            var schedule = await response.Content.ReadAsAsync<ScheduleRun>();
            return schedule;
        }

        public async Task UpdateBatchDataInBatchLog(Guid batchId, BatchData batchData)
        {
            await Client.PutAsJsonAsync($"/api/v1/batches/update-batch-data", new { Id = batchId, BatchData = batchData });
        }

        public async Task<EMailTemplate> GetEmailTemplate(string templateName)
        {
            var response = await Client.GetAsync($"/api/v1/emailTemplates/{templateName}");

            var emailTemplate = await response.Content.ReadAsAsync<EMailTemplate>();
            if (response.IsSuccessStatusCode)
            {
                Logger.Log(LogLevel.Information, $"Status code returned: {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
            }
            else
            {
                Logger.Log(LogLevel.Information, $"Status code returned: {response.StatusCode}. Content: {response.Content.ReadAsStringAsync().Result}");
            }

            return emailTemplate;
        }

    }
}
