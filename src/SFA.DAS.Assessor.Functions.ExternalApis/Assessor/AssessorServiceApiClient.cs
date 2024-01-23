using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
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

        public async Task<BatchLogResponse> GetBatchLog(int batchNumber)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/{batchNumber}"))
            {
                return await GetAsync<BatchLogResponse>(request);
            }
        }

        public async Task<int> UpdateBatchLogReadyToPrintAddCertifictes(int batchNumber, int maxCertificatesToBeAdded)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/{batchNumber}/update-ready-to-print-add-certificates"))
            {
                return await PostPutRequestWithResponse<UpdateBatchLogReadyToPrintAddCertificatesRequest, int>(
                    request, 
                    new UpdateBatchLogReadyToPrintAddCertificatesRequest { MaxCertificatesToBeAdded = maxCertificatesToBeAdded });
            }
        }

        public async Task<ValidationResponse> UpdateBatchLogSentToPrinter(int batchNumber, UpdateBatchLogSentToPrinterRequest model)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/{batchNumber}/update-sent-to-printer"))
            {
                return await PostPutRequestWithResponse<UpdateBatchLogSentToPrinterRequest, ValidationResponse>(request, model);
            }
        }

        public async Task<ValidationResponse> UpdateBatchLogPrinted(int batchNumber, UpdateBatchLogPrintedRequest model)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/batches/{batchNumber}/update-printed"))
            {
                return await PostPutRequestWithResponse<UpdateBatchLogPrintedRequest, ValidationResponse>(request, model);
            }
        }

        public async Task<int?> GetBatchNumberReadyToPrint()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/batches/batch-number-ready-to-print"))
            {
                return await GetAsync<int?>(request);
            }
        }

        public async Task<CertificatesForBatchNumberResponse> GetCertificatesForBatchNumber(int batchNumber)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/certificates/batch/{batchNumber}"))
            {
                return await GetAsync<CertificatesForBatchNumberResponse>(request);
            }
        }

        public async Task<int> GetCertificatesReadyToPrintCount()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/certificates/ready-to-print/count"))
            {
                return await GetAsync<int>(request);
            }
        }

        public async Task<ValidationResponse> UpdateCertificatesPrintStatus(CertificatesPrintStatusUpdateRequest model)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/certificates/update-print-status"))
            {
                return await PostPutRequestWithResponse<CertificatesPrintStatusUpdateRequest, ValidationResponse>(request, model);
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

        public async Task UpdateLastRunStatus(UpdateLastRunStatusRequest updateLastRunStatusRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/schedule/updatelaststatus"))
            {
                await PostPutRequest(request, updateLastRunStatusRequest);
            }
        }

        public async Task UpdateStandards()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/standard-version/update-standards"))
            {
                // no retry as this should not run more than once a day, this is a background task
                // which will return 202 immediately as it takes a long time to complete on production data
                await PostRequestWithoutRetry(request);
            }
        }

        public async Task RebuildExternalApiSandbox()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/externalapidatasync/rebuild-sandbox"))
            {
                // no retry as this should not run more than once a day, this is a background task
                // which will return 202 immediately as it takes a long time to complete on production data
                await PostRequestWithoutRetry(request);
            }
        }

        public async Task ImportLearners()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/approvals/update-approvals"))
            {
                // no retry as this should not run more than once a day, this is a background task
                // which will return 202 immediately as it takes a long time to complete on production data
                await PostRequestWithoutRetry(request);
            }
        }

        public async Task RefreshProviders()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/providers/refresh-providers"))
            {
                // no retry as this should not run more than once a day, this is a background task
                // which will return 202 immediately as it takes a long time to complete on production data
                await PostRequestWithoutRetry(request);
            }
        }

        public async Task UpdateStandardSummary()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/oppfinder/update-standard-summary"))
            {
                // no retry as this should not run more than once a day, this is a background task
                // which will return 202 immediately as it takes a long time to complete on production data
                await PostRequestWithoutRetry(request);
            }
        }

        public async Task AparSummaryUpdate()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/ao/assessment-organisations/apar-summary-update"))
            {
                // no retry as this should not run more than once a day, this is a background task
                // which will return 202 immediately as it takes a long time to complete on production data
                await PostRequestWithoutRetry(request);
            }
        }
    }
}
