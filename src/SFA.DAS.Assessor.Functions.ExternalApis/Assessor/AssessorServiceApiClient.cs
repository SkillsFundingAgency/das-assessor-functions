using Microsoft.Extensions.Logging;
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
            IOptions<AssessorApiAuthentication> options, 
            ILogger<AssessorServiceApiClient> logger)
            : base(httpClient, new Uri(options?.Value.ApiBaseAddress), logger)
        {
        }
        public async Task UpdateStandards()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/ao/update-standards"))
            {
                await PostPutRequest(request, new { });
            }
        }

        public async Task UpdateStandardSummary()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/oppfinder/update-standard-summary"))
            {
                await PostPutRequest(request, new { });
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

        public async Task<BatchLogResponse> CreateBatchLog(CreateBatchLogRequest createBatchLogRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/batches/create"))
            {
                return await PostPutRequestWithResponse<CreateBatchLogRequest, BatchLogResponse>(request, createBatchLogRequest);
            }
        }

        public async Task SaveSentToPrinter(int batchNumber, IEnumerable<string> certificateReferences)
        {
            // the certificate printed status be will updated in chunks to stay within the WAF message size limits
            const int chunkSize = 100;

            foreach (var certificateReferencesChunk in certificateReferences.ToList().ChunkBy(chunkSize))
            {
                var updateBatchLogSentToPrinterRequest = new UpdateBatchLogSentToPrinterRequest
                {
                    BatchNumber = batchNumber,
                    CertificateReferences = certificateReferencesChunk
                };

                using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/sent-to-printer"))
                {
                    await PostPutRequest(request, updateBatchLogSentToPrinterRequest);
                }
            }
        }

        public async Task UpdateBatchToPrinted(int batchNumber, DateTime printedDateTime)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/printed "))
            {
                await PostPutRequest(request, new { batchNumber, printedAt = printedDateTime });
            }
        }

        public async Task UpdatePrintStatus(IEnumerable<CertificatePrintStatus> certificatePrintStatus)
        {
            // the certificate printed status be will updated in chunks to stay within the WAF message size limits
            const int chunkSize = 100;

            foreach (var certificatePrintStatusChunk in certificatePrintStatus.ToList().ChunkBy(chunkSize))
            {
                var updateCertificatesPrintStatusRequest = new UpdateCertificatesPrintStatusRequest
                {
                     CertificatePrintStatuses = certificatePrintStatusChunk
                };

                using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/certificates/update-print-status"))
                {
                    await PostPutRequest(request, updateCertificatesPrintStatusRequest);
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

        public async Task<CertificatesToBePrintedResponse> GetCertificatesToBePrinted()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/certificates/tobeprinted"))
            {
                return await GetAsync<CertificatesToBePrintedResponse>(request);
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

        public async Task<BatchLogResponse> GetGetBatchLogByBatchNumber(string batchNumber)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/{batchNumber}"))
            {
                return await GetAsync<BatchLogResponse>(request);
            }
        }

        public async Task<ScheduleRun> GetSchedule(ScheduleType scheduleType)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/schedule/runnow?scheduleType={(int)scheduleType}"))
            {
                return await GetAsync<ScheduleRun>(request);
            }
        }

        public async Task UpdateBatchDataInBatchLog(Guid batchId, BatchData batchData)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/update-batch-data"))
            {
                await PostPutRequest(request, new { Id = batchId, BatchData = batchData });
            }
        }

        public async Task<EmailTemplateSummary> GetEmailTemplate(string templateName)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/emailTemplates/{templateName}"))
            {
                return await GetAsync<EmailTemplateSummary>(request);
            }
        }

        public async Task SendEmailWithTemplate(SendEmailRequest sendEmailRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/emailTemplates"))
            {                
                await PostPutRequest(request, sendEmailRequest);
            }
        }
    }
}
    