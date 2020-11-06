using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class BatchService : IBatchService
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;
        private readonly ILogger<BatchService> _logger;

        public BatchService(IAssessorServiceApiClient assessorServiceApiClient, ILogger<BatchService> logger)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public async Task<Batch> Get(int batchNumber)
        {
            var batchLogResponse = await _assessorServiceApiClient.GetBatchLog(batchNumber);

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

        public async Task<int?> BuildPrintBatchReadyToPrint(DateTime scheduledDate, int maxCertificatesToBeAdded)
        {
            var nextBatchNumberReadyToPrint = await _assessorServiceApiClient.GetBatchNumberReadyToPrint();
            if (await _assessorServiceApiClient.GetCertificatesReadyToPrintCount() > 0)
            {
                if (nextBatchNumberReadyToPrint == null)
                {
                    var response = await _assessorServiceApiClient.CreateBatchLog(new CreateBatchLogRequest
                    {
                        ScheduledDate = scheduledDate
                    });

                    nextBatchNumberReadyToPrint = response.BatchNumber;
                }

                do
                {
                    var model = new UpdateBatchLogReadyToPrintAddCertificatesRequest()
                    {
                        MaxCertificatesToBeAdded = maxCertificatesToBeAdded
                    };

                    var addedCount = await _assessorServiceApiClient.UpdateBatchLogReadyToPrintAddCertifictes(nextBatchNumberReadyToPrint.Value, model);

                    _logger.LogInformation($"Added {addedCount} ready to print certificates to batch {nextBatchNumberReadyToPrint.Value}");
                }
                while (await _assessorServiceApiClient.GetCertificatesReadyToPrintCount() > 0);
            }

            return nextBatchNumberReadyToPrint;
        }

        public async Task<List<Certificate>> GetCertificatesForBatchNumber(int batchNumber)
        {
            var response = await _assessorServiceApiClient.GetCertificatesForBatchNumber(batchNumber);
            return response.Certificates.Select(Map).ToList();
        }

        public async Task Update(Batch batch, ICollector<string> storageQueue, int maxCertificatesToUpdate)
        {
            if (batch.Status == CertificateStatus.SentToPrinter)
            {
                _logger.LogInformation($"Requested batch log {batch.BatchNumber} is updated to sent to printer");

                var updateRequest = new UpdateBatchLogSentToPrinterRequest()
                {
                    BatchCreated = batch.BatchCreated,
                    NumberOfCertificates = batch.NumberOfCertificates,
                    NumberOfCoverLetters = batch.NumberOfCoverLetters,
                    CertificatesFileName = batch.CertificatesFileName,
                    FileUploadStartTime = batch.FileUploadStartTime,
                    FileUploadEndTime = batch.FileUploadEndTime,
                };

                var response = await _assessorServiceApiClient.UpdateBatchLogSentToPrinter(batch.BatchNumber, updateRequest);
                if (response.Errors.Count == 0)
                {
                    var messages = QueueCertificatePrintStatusUpdateMessages(batch.BatchNumber, batch.Certificates, batch.Status, DateTime.UtcNow, maxCertificatesToUpdate);
                    messages.ForEach(p => storageQueue.Add(p));

                    _logger.LogInformation($"Queued {messages.Count} messages for batch log {batch.BatchNumber} to update {batch.Certificates.Count} certificates as sent to printer");
                }
            }
            else if (batch.Status == CertificateStatus.Printed)
            {
                _logger.LogInformation($"Requested batch log {batch.BatchNumber} is updated as printed");

                var updateRequest = new UpdateBatchLogPrintedRequest()
                {
                    BatchDate = batch.BatchCreated,
                    PostalContactCount = batch.NumberOfCoverLetters,
                    TotalCertificateCount = batch.NumberOfCertificates,
                    PrintedDate = batch.PrintedDate,
                    DateOfResponse = batch.DateOfResponse
                };

                var response = await _assessorServiceApiClient.UpdateBatchLogPrinted(batch.BatchNumber, updateRequest);
                if (response.Errors.Count == 0)
                {
                    var messages = QueueCertificatePrintStatusUpdateMessages(batch.BatchNumber, batch.Certificates, batch.Status, batch.PrintedDate.Value, maxCertificatesToUpdate);
                    messages.ForEach(p => storageQueue.Add(p));

                    _logger.LogInformation($"Queued {messages.Count} messages for batch log {batch.BatchNumber} to update {batch.Certificates.Count} certificates as printed");
                }
            }
        }
        
        private List<string> QueueCertificatePrintStatusUpdateMessages(int batchNumber, List<Certificate> certificates, string status, DateTime statusAt, int maxCertificatesToUpdate)
        {
            var messages = new List<string>();

            foreach (var chunk in certificates.ChunkBy(maxCertificatesToUpdate))
            {
                var message = new CertificatePrintStatusUpdateMessage()
                {
                    CertificatePrintStatusUpdates = chunk.Select(p => new CertificatePrintStatusUpdate()
                    {
                        CertificateReference = p.CertificateReference,
                        BatchNumber = batchNumber,
                        Status = status,
                        StatusAt = statusAt,
                        ReasonForChange = null
                    }).ToList()
                };

                messages.Add(JsonConvert.SerializeObject(message));
            }

            return messages;
        }

        private Certificate Map(CertificatePrintSummary certificateToBePrinted)
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
