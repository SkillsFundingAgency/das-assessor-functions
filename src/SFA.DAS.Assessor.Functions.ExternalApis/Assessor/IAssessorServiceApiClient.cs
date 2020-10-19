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
        Task CompleteSchedule(Guid scheduleRunId);
        Task<CertificatesToBePrintedResponse> GetCertificatesToBePrinted();
        Task<BatchLogResponse> GetCurrentBatchLog();
        Task<BatchLogResponse> GetGetBatchLogByBatchNumber(string batchNumber);
        Task<ScheduleRun> GetSchedule(ScheduleType scheduleType);
        Task UpdateBatchDataInBatchLog(Guid batchId, BatchData batchData);
        Task<ValidationResponse> SaveSentToPrinter(int batchNumber, IEnumerable<string> certificateReferences);
        Task<ValidationResponse> UpdateBatchToPrinted(int batchNumber, DateTime printedDateTime);
        Task<ValidationResponse> UpdatePrintStatus(IEnumerable<CertificatePrintStatus> certificatePrintStatus);        
        Task<EmailTemplateSummary> GetEmailTemplate(string templateName);
        Task SendEmailWithTemplate(SendEmailRequest sendEmailRequest);
    }
}
