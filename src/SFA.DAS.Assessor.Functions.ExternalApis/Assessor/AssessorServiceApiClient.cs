﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ApiClient.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task UpdateStandardSummary()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/oppfinder/update-standard-summary"))
            {
                return PostPutRequest(request);
            }
        }

        public Task SetAssessorSetting(string name, string value)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/assessor-setting/{name}/{value}"))
            {
               return PostPutRequest(request);
            }
        }

        public Task<string> GetAssessorSetting(string name)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/assessor-setting/{name}"))
            {
                return GetAsync<string>(request);
            }
        }

        public string BaseAddress()
        {
            return Client.BaseAddress.ToString();
        }

        public Task<BatchLogResponse> CreateBatchLog(CreateBatchLogRequest createBatchLogRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/batches"))
            {
                return PostPutRequestWithResponse<CreateBatchLogRequest, BatchLogResponse>(request, createBatchLogRequest);
            }
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

                using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/certificates/{batchNumber}"))
                {
                    await PostPutRequest(request, updateCertificatesBatchToIndicatePrintedRequest);
                }
            }
        }

        public async Task CompleteSchedule(Guid scheduleRunId)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/schedule?scheduleRunId={scheduleRunId}"))
            {
                await PostPutRequest(request);
            }
        }

        public Task<IEnumerable<CertificateResponse>> GetCertificatesToBePrinted()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/certificates/tobeprinted"))
            {
                return GetAsync<IEnumerable<CertificateResponse>>(request);
            }
        }

        public async Task<BatchLogResponse> GetCurrentBatchLog()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/batches/latest"))
            {
                var response = await GetAsync<BatchLogResponse>(request);

                if(response == null)
                {
                    return new BatchLogResponse
                    {
                        BatchNumber = 0
                    };
                }

                return response;
            }
        }

        public Task<BatchLogResponse> GetGetBatchLogByBatchNumber(string batchNumber)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/{batchNumber}"))
            {
                return GetAsync<BatchLogResponse>(request);
            }
        }

        public Task<ScheduleRun> GetSchedule(ScheduleType scheduleType)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/schedule/runnow?scheduleType={(int)scheduleType}"))
            {
                return GetAsync<ScheduleRun>(request);
            }
        }

        public Task UpdateBatchDataInBatchLog(Guid batchId, BatchData batchData)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/update-batch-data"))
            {
                return PostPutRequest(request, new { Id = batchId, BatchData = batchData });
            }
        }

        public Task<EMailTemplate> GetEmailTemplate(string templateName)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/emailTemplates/{templateName}"))
            {
                return GetAsync<EMailTemplate>(request);
            }
        }
    }
}
