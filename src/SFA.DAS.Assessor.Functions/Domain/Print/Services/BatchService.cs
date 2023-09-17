using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

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
            if (maxCertificatesToBeAdded <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCertificatesToBeAdded), maxCertificatesToBeAdded, "The value must be greater than zero");
            }

            var nextBatchNumberReadyToPrint = await GetExistingReadyToPrintBatchNumber();
            if (await ReadyToPrintCertificatesNotInBatch())
            {
                if (!nextBatchNumberReadyToPrint.HasValue)
                {
                    nextBatchNumberReadyToPrint = await CreateNewBatchNumber(scheduledDate);
                }

                do
                {
                    var addedCount = await _assessorServiceApiClient.UpdateBatchLogReadyToPrintAddCertifictes(
                        nextBatchNumberReadyToPrint.Value,
                        maxCertificatesToBeAdded);

                    _logger.LogInformation($"Added {addedCount} ready to print certificates to batch {nextBatchNumberReadyToPrint.Value}");
                }
                while (await ReadyToPrintCertificatesNotInBatch());
            }

            if (nextBatchNumberReadyToPrint.HasValue)
            {
                var batch = await Get(nextBatchNumberReadyToPrint.Value);
                batch.Certificates = await GetCertificatesForBatchNumber(nextBatchNumberReadyToPrint.Value);
                return batch;
            }

            return null;
        }

        public async Task<List<Certificate>> GetCertificatesForBatchNumber(int batchNumber)
        {
            var response = await _assessorServiceApiClient.GetCertificatesForBatchNumber(batchNumber);
            if (response == null)
            {
                throw new Exception($"Unable to get the certificates for batch number {batchNumber}");
            }

            return response.Certificates?.Select(c => Certificate.FromCertificatePrintSummary(c))
                                         .ToList();
        }

        public async Task<List<CertificatePrintStatusUpdateMessage>> Update(Batch batch)
        {
            List<CertificatePrintStatusUpdateMessage> printStatusUpdateMessages = new List<CertificatePrintStatusUpdateMessage>();

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

            if (printStatusUpdateMessages.Count > 0)
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

        private List<CertificatePrintStatusUpdateMessage> BuildCertificatePrintStatusUpdateMessages(int batchNumber, List<Certificate> certificates, string status, DateTime statusAt)
        {
            var messages = new List<CertificatePrintStatusUpdateMessage>();

            certificates.ForEach(p =>
            {
                var message = new CertificatePrintStatusUpdateMessage
                {
                    CertificateReference = p.CertificateReference,
                    BatchNumber = batchNumber,
                    Status = status,
                    StatusAt = statusAt,
                    ReasonForChange = null
                };

                messages.Add(message);
            });

            return messages;
        }
    }
}
