using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor
{
    public interface IAssessorServiceApiClient : IApiClientBase
    {
        Task UpdateStandards();
        Task UpdateStandardSummary();
        Task SetAssessorSetting(string name, string value);
        Task<string> GetAssessorSetting(string name);

        Task<BatchLogResponse> CreateBatchLog(CreateBatchLogRequest createBatchLogRequest);
        Task<BatchLogResponse> GetBatchLog(int batchNumber);
        Task<int> UpdateBatchLogReadyToPrintAddCertifictes(int batchNumber, int maxCertificatesToBeAdded);
        Task<ValidationResponse> UpdateBatchLogSentToPrinter(int batchNumber, UpdateBatchLogSentToPrinterRequest model);
        Task<ValidationResponse> UpdateBatchLogPrinted(int batchNumber, UpdateBatchLogPrintedRequest model);
        Task<int?> GetBatchNumberReadyToPrint();

        Task<CertificatesForBatchNumberResponse> GetCertificatesForBatchNumber(int batchNumber);
        Task<int> GetCertificatesReadyToPrintCount();
        Task<ValidationResponse> UpdateCertificatesPrintStatus(CertificatesPrintStatusUpdateRequest model);

        Task CompleteSchedule(Guid scheduleRunId);
        Task<ScheduleRun> GetSchedule(ScheduleType scheduleType);

        Task<EmailTemplateSummary> GetEmailTemplate(string templateName);
        Task SendEmailWithTemplate(SendEmailRequest sendEmailRequest);

        Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest request);
        Task UpdateLastRunStatus(UpdateLastRunStatusRequest updateScheduleRunStatusRequest);
        Task RebuildExternalApiSandbox();
        Task ImportLearners();
        Task RefreshProviders();
    }
}