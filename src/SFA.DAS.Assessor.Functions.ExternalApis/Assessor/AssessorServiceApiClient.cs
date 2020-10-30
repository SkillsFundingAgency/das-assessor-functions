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

        public async Task<BatchLogResponse> GetBatchLogByBatchNumber(string batchNumber)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/{batchNumber}"))
            {
                return await GetAsync<BatchLogResponse>(request);
            }
        }

        public async Task UpdateBatchAddCertificatesReadyToPrint(int batchNumber)
        {
            var chunkSize = 50;

            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/{batchNumber}/add-certificates-ready-to-print/{chunkSize}"))
            {
                BatchReadyToPrintResponse response = null;
                do
                {
                    response = await PostPutRequestWithResponse<UpdateBatchLogAddCertificatesReadyToPrintRequest, BatchReadyToPrintResponse>(request, null);
                } while (response != null && response.CertificatesAdded == chunkSize);
            }
        }

        public async Task<int?> GetNextBatchNumberToBePrinted()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/next-ready-to-print"))
            {
                return await GetAsync<int?>(request);
            }
        }

        public async Task<CertificatesToBePrintedResponse> GetCertificatesToBePrinted(int batchNumber)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/{batchNumber}/certificates-ready-to-print"))
            {
                return await GetAsync<CertificatesToBePrintedResponse>(request);
            }
        }

        public async Task UpdateBatchDataInBatchLog(int batchNumber, BatchData batchData)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/{batchNumber}/update-batch-data"))
            {
                await PostPutRequest(request, new { BatchData = batchData });
            }
        }

        public async Task<int> GetCertificatesReadyToPrintCount()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/certificates/ready-to-print/count"))
            {
                return await GetAsync<int>(request);
            }
        }

        public async Task UpdatePrintStatus(List<CertificatePrintStatusUpdate> certificatePrintStatusChanges)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/certificates/update-print-status"))
            {
                await PostPutRequest(request, certificatePrintStatusChanges);
            }
        }

        public async Task CompleteSchedule(Guid scheduleRunId)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/schedule?scheduleRunId={scheduleRunId}"))
            {
                await PostPutRequest(request);
            }
        }
        
        public async Task<ScheduleRun> GetSchedule(ScheduleType scheduleType)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/schedule/runnow?scheduleType={(int)scheduleType}"))
            {
                return await GetAsync<ScheduleRun>(request);
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

        public async Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest importLearnerDetailRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/learnerdetails/import"))
            {
                return await PostPutRequestWithResponse<ImportLearnerDetailRequest, ImportLearnerDetailResponse>(request,
                    importLearnerDetailRequest);
            }
        }
    }
}
