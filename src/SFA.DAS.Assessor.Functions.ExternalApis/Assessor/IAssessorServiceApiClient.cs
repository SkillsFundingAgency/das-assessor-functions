using SFA.DAS.Assessor.Functions.ApiClient.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
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
        Task<BatchLogResponse> GetBatchLogByBatchNumber(string batchNumber);
        Task UpdateBatchAddCertificatesReadyToPrint(int batchNumber);
        Task<int?> GetNextBatchNumberToBePrinted();
        Task<CertificatesToBePrintedResponse> GetCertificatesToBePrinted(int batchNumber);
        Task UpdateBatchDataInBatchLog(int batchNumber, BatchData batchData);
        Task<int> GetCertificatesReadyToPrintCount();
        Task UpdatePrintStatus(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates);
        Task CompleteSchedule(Guid scheduleRunId);
        Task<ScheduleRun> GetSchedule(ScheduleType scheduleType);
        Task<EmailTemplateSummary> GetEmailTemplate(string templateName);
        Task SendEmailWithTemplate(SendEmailRequest sendEmailRequest);
        Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest request);
    }
}