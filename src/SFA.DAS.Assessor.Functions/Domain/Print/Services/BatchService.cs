using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
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

        public async Task<Batch> BuildPrintBatchReadyToPrint(DateTime scheduledDate, int maxCertificatesToBeAdded)
        {
            var nextBatchNumberReadyToPrint = await GetExistingReadyToPrintBatchNumber();
            if (await ReadyToPrintCertificatesNotInBatch())
            {
                if (!nextBatchNumberReadyToPrint.HasValue)
                {
                    nextBatchNumberReadyToPrint = await CreateNewBatchNumber(scheduledDate);
                }

                if (nextBatchNumberReadyToPrint.HasValue)
                {
                    do
                    {
                        var addedCount = await _assessorServiceApiClient.UpdateBatchLogReadyToPrintAddCertifictes(
                            nextBatchNumberReadyToPrint.Value,
                            maxCertificatesToBeAdded);

                        _logger.LogInformation($"Added {addedCount} ready to print certificates to batch {nextBatchNumberReadyToPrint.Value}");
                    }
                    while (await ReadyToPrintCertificatesNotInBatch());
                }
                else
                {
                    _logger.LogError($"Unable to create a new batch log for scheduled date {scheduledDate}");
                }
            }

            if(nextBatchNumberReadyToPrint.HasValue)
            {
                var batch = await Get(nextBatchNumberReadyToPrint.Value);
                batch.Certificates = (await GetCertificatesForBatchNumber(nextBatchNumberReadyToPrint.Value)).Sanitise(_logger);
                return batch;
            }

            return null;
        }

        public async Task<List<Certificate>> GetCertificatesForBatchNumber(int batchNumber)
        {
            var response = await _assessorServiceApiClient.GetCertificatesForBatchNumber(batchNumber);
            return response.Certificates.Select(Map).ToList();
        }

        public async Task<List<string>> Update(Batch batch)
        {
            List<string> printStatusUpdateMessages = new List<string>();

            if (batch.Status == CertificateStatus.SentToPrinter)
            {
                _logger.LogInformation($"Batch log {batch.BatchNumber} will be updated as sent to printer");

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
                    printStatusUpdateMessages.AddRange(BuildCertificatePrintStatusUpdateMessages(
                        batch.BatchNumber, batch.Certificates, batch.Status, DateTime.UtcNow));
                }
            }
            else if (batch.Status == CertificateStatus.Printed)
            {
                _logger.LogInformation($"Batch log {batch.BatchNumber} will be updated as printed");

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
                    printStatusUpdateMessages.AddRange(BuildCertificatePrintStatusUpdateMessages(
                        batch.BatchNumber, batch.Certificates, batch.Status, batch.PrintedDate.Value));
                }
            }

            if(printStatusUpdateMessages.Count > 0)
            {
                _logger.LogInformation($"Batch log {batch.BatchNumber} contained {batch.Certificates.Count} certificates, for which {printStatusUpdateMessages.Count} messages will be queued");
            }

            return printStatusUpdateMessages;
        }

        private async Task<int?> GetExistingReadyToPrintBatchNumber()
        {
            return await _assessorServiceApiClient.GetBatchNumberReadyToPrint();
        }

        private async Task<bool> ReadyToPrintCertificatesNotInBatch()
        {
            return (await _assessorServiceApiClient.GetCertificatesReadyToPrintCount() > 0);
        }

        private async Task<int> CreateNewBatchNumber(DateTime scheduledDate)
        {
            var response = await _assessorServiceApiClient.CreateBatchLog(new CreateBatchLogRequest
            {
                ScheduledDate = scheduledDate
            });

            return response.BatchNumber;
        }

        private List<string> BuildCertificatePrintStatusUpdateMessages(int batchNumber, List<Certificate> certificates, string status, DateTime statusAt)
        {
            var messages = new List<string>();

            certificates.ForEach(p =>
            {
                var message = JsonConvert.SerializeObject(new CertificatePrintStatusUpdate
                {
                    CertificateReference = p.CertificateReference,
                    BatchNumber = batchNumber,
                    Status = status,
                    StatusAt = statusAt,
                    ReasonForChange = null
                });

                messages.Add(message);
            });

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
