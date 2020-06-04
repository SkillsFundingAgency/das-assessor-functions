using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class BatchClient : IBatchClient
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;

        public BatchClient(IAssessorServiceApiClient assessorServiceApiClient)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
        }

        public async Task<Batch> Get(int batchNumber)
        {
            var batchLogResponse = await _assessorServiceApiClient.GetGetBatchLogByBatchNumber(batchNumber.ToString());

            if (batchLogResponse?.Id != null)
            {
                return new Batch
                {
                    Id = batchLogResponse.Id,
                    BatchNumber = batchLogResponse.BatchNumber,
                    FileUploadStartTime = batchLogResponse.FileUploadStartTime,
                    Period = batchLogResponse.Period,
                    BatchCreated = batchLogResponse.BatchCreated,
                    ScheduledDate = batchLogResponse.ScheduledDate,
                    CertificatesFileName = batchLogResponse.CertificatesFileName,
                    FileUploadEndTime = batchLogResponse.FileUploadEndTime,
                    NumberOfCertificates = batchLogResponse.NumberOfCertificates,
                    NumberOfCoverLetters = batchLogResponse.NumberOfCoverLetters
                };
            }

            return null;
        }

        public async Task<int> NextBatchId()
        {
            var response = await _assessorServiceApiClient.GetCurrentBatchLog();

            return response.BatchNumber + 1;
        }
        public async Task Save(Batch batch)
        {
            if (!batch.Id.HasValue) // is new
            {
                await _assessorServiceApiClient.CreateBatchLog(new CreateBatchLogRequest
                {
                    BatchNumber = batch.BatchNumber,
                    FileUploadStartTime= batch.FileUploadStartTime,
                    Period = batch.Period,
                    BatchCreated = batch.BatchCreated,
                    ScheduledDate = batch.ScheduledDate,
                    CertificatesFileName = batch.CertificatesFileName,
                    FileUploadEndTime = batch.FileUploadEndTime,
                    NumberOfCertificates = batch.NumberOfCertificates,
                    NumberOfCoverLetters = batch.NumberOfCoverLetters
                });

                await _assessorServiceApiClient.SaveSentToPrinter(batch.BatchNumber, batch.Certificates.Select(c => c.CertificateReference));

            }
            else // update existing
            {
                await _assessorServiceApiClient.UpdateBatchDataInBatchLog(
                    batch.Id.Value,
                    new BatchData
                    {
                        BatchNumber = batch.BatchNumber,
                        BatchDate = batch.BatchCreated,
                        PostalContactCount = batch.NumberOfCoverLetters,
                        TotalCertificateCount = batch.NumberOfCertificates,
                        PrintedDate = batch.PrintedDate,
                        PostedDate = batch.PostedDate,
                        DateOfResponse = batch.DateOfResponse
                    });
            }

            if (batch.Status == "Printed")
            {
                await _assessorServiceApiClient.UpdateBatchToPrinted(batch.BatchNumber, batch.PrintedDate ?? DateTime.Now);
            }
        }
    }
}
