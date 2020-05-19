using SFA.DAS.Assessor.Functions.ApiClient.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor
{
    public interface IAssessorServiceApiClient : IApiClientBase
    {
        Task UpdateStandardSummary();
        Task SetAssessorSetting(string name, string value);
        Task<string> GetAssessorSetting(string name);
        Task<ImportLearnerDetailResponse> ImportLearnerDetails(ImportLearnerDetailRequest request);
        Task<BatchLogResponse> CreateBatchLog(CreateBatchLogRequest createBatchLogRequest);
        Task ChangeStatusToPrinted(int batchNumber, IEnumerable<CertificateResponse> responses);
        Task CompleteSchedule(Guid scheduleRunId);
        Task<IEnumerable<CertificateResponse>> GetCertificatesToBePrinted();
        Task<BatchLogResponse> GetCurrentBatchLog();
        Task<BatchLogResponse> GetGetBatchLogByBatchNumber(string batchNumber);
        Task<ScheduleRun> GetSchedule(ScheduleType scheduleType);
        Task UpdateBatchDataInBatchLog(Guid batchId, BatchData batchData);
        Task<EMailTemplate> GetEmailTemplate(string templateName);
    }
}
