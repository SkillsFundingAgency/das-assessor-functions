using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class BatchService : IBatchService
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;

        public BatchService(IAssessorServiceApiClient assessorServiceApiClient)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
        }

        public async Task<Batch> Get(int batchNumber)
        {
            var batchLogResponse = await _assessorServiceApiClient.GetBatchLogByBatchNumber(batchNumber.ToString());

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

        public async Task<int?> CreateNextBatchToBePrinted(DateTime scheduledDate)
        {
            if (await _assessorServiceApiClient.GetCertificatesReadyToPrintCount() > 0)
            {
                var response = await _assessorServiceApiClient.CreateBatchLog(new CreateBatchLogRequest
                {
                    ScheduledDate = scheduledDate
                });

                await _assessorServiceApiClient.UpdateBatchAddCertificatesReadyToPrint(response.BatchNumber);
            }

            return await GetNextBatchNumberToBePrinted();
        }

        public async Task<int?> GetNextBatchNumberToBePrinted()
        {
            return await _assessorServiceApiClient.GetNextBatchNumberToBePrinted();
        }

        public async Task<List<Certificate>> GetCertificatesToBePrinted(int batchNumber)
        {
            var response = await _assessorServiceApiClient.GetCertificatesToBePrinted(batchNumber);
            return response.Certificates.Select(Map).ToList();
        }

        public async Task Update(Batch batch, ICollector<string> storageQueue)
        {
            foreach (var chunk in batch.Certificates.ChunkBy(10))
            {
                var message = new CertificatePrintStatusUpdateMessage()
                {
                    CertificatePrintStatusUpdates = chunk.Select(p => new CertificatePrintStatusUpdate()
                    {
                        CertificateReference = p.CertificateReference,
                        BatchNumber = batch.BatchNumber,
                        Status = batch.Status,
                        StatusAt = batch.Status == "Printed" ? batch.PrintedDate.Value : DateTime.UtcNow,
                        ReasonForChange = null
                    }).ToList()
                };

                storageQueue.Add(JsonConvert.SerializeObject(message));
            }

            await _assessorServiceApiClient.UpdateBatchDataInBatchLog(
                batch.BatchNumber,
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

        private Certificate Map(CertificateToBePrintedSummary certificateToBePrinted)
        {
            var certificate = new Certificate
            {
                Uln = certificateToBePrinted.Uln,
                StandardCode = certificateToBePrinted.StandardCode,
                ProviderUkPrn = certificateToBePrinted.ProviderUkPrn,
                EndPointAssessorOrganisationId = certificateToBePrinted.EndPointAssessorOrganisationId,
                EndPointAssessorOrganisationName = certificateToBePrinted.EndPointAssessorOrganisationName,
                CertificateReference = certificateToBePrinted.CertificateReference,
                LearnerGivenNames = certificateToBePrinted.LearnerGivenNames,
                LearnerFamilyName = certificateToBePrinted.LearnerFamilyName,
                StandardName = certificateToBePrinted.StandardName,
                StandardLevel = certificateToBePrinted.StandardLevel,
                ContactName = certificateToBePrinted.ContactName,
                ContactOrganisation = certificateToBePrinted.ContactOrganisation,
                ContactAddLine1 = certificateToBePrinted.ContactAddLine1,
                ContactAddLine2 = certificateToBePrinted.ContactAddLine2,
                ContactAddLine3 = certificateToBePrinted.ContactAddLine3,
                ContactAddLine4 = certificateToBePrinted.ContactAddLine4,
                ContactPostCode = certificateToBePrinted.ContactPostCode,
                AchievementDate = certificateToBePrinted.AchievementDate,
                CourseOption = certificateToBePrinted.CourseOption,
                OverallGrade = certificateToBePrinted.OverallGrade,
                Department = certificateToBePrinted.Department,
                FullName = certificateToBePrinted.FullName,
                Status = certificateToBePrinted.Status
            };

            return certificate;
        }
    }
}
