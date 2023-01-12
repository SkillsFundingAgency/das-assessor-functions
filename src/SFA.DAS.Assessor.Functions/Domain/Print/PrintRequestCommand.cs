using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintRequestCommand : IPrintRequestCommand
    {
        private readonly ILogger<PrintRequestCommand> _logger;
        private readonly IPrintCreator _printCreator;
        private readonly IBatchService _batchService;
        private readonly IScheduleService _scheduleService;

        private readonly INotificationService _notificationService;
        private readonly IExternalBlobFileTransferClient _externalFileTransferClient;
        private readonly IInternalBlobFileTransferClient _internalFileTransferClient;
        private readonly PrintRequestOptions _options;

        public PrintRequestCommand(
            ILogger<PrintRequestCommand> logger,
            IPrintCreator printCreator,
            IBatchService batchService,
            IScheduleService scheduleService,
            INotificationService notificationService,
            IExternalBlobFileTransferClient externalFileTransferClient,
            IInternalBlobFileTransferClient internalFileTransferClient,
            IOptions<PrintRequestOptions> options)
        {
            _logger = logger;
            _printCreator = printCreator;
            _batchService = batchService;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _externalFileTransferClient = externalFileTransferClient;
            _internalFileTransferClient = internalFileTransferClient;
            _options = options?.Value;
        }

        public async Task<List<CertificatePrintStatusUpdateMessage>> Execute()
        {
            Schedule schedule = null;
            List<CertificatePrintStatusUpdateMessage> printStatusUpdateMessages = null;

            try
            {
                _logger.LogInformation("PrintRequestCommand - Started");

                schedule = await _scheduleService.Get();
                if (schedule == null)
                {
                    _logger.LogInformation("PrintRequestCommand - There is no print schedule which allows printing at this time");
                    return null;
                }

                await _scheduleService.Start(schedule);

                var nextBatchReadyToPrint = await _batchService.BuildPrintBatchReadyToPrint(schedule.RunTime, _options.AddReadyToPrintLimit);
                if (nextBatchReadyToPrint != null)
                {
                    if ((nextBatchReadyToPrint.Certificates?.Count ?? 0) == 0)
                    {
                        _logger.LogInformation($"PrintRequestCommand - There are no certificates in batch number {nextBatchReadyToPrint} ready to print at this time");
                    }
                    else
                    {
                        nextBatchReadyToPrint.Status = CertificateStatus.SentToPrinter;
                        nextBatchReadyToPrint.BatchCreated = DateTime.UtcNow;
                        nextBatchReadyToPrint.CertificatesFileName = GetCertificatesFileName(nextBatchReadyToPrint.BatchNumber, nextBatchReadyToPrint.BatchCreated);

                        var printOutput = _printCreator.Create(nextBatchReadyToPrint.BatchNumber, nextBatchReadyToPrint.Certificates);

                        var fileContents = JsonConvert.SerializeObject(printOutput);
                        nextBatchReadyToPrint.NumberOfCertificates = printOutput.Batch.TotalCertificateCount;
                        nextBatchReadyToPrint.NumberOfCoverLetters = printOutput.Batch.PostalContactCount;

                        nextBatchReadyToPrint.FileUploadStartTime = DateTime.UtcNow;
                        var uploadDirectory = _options.Directory;
                        var uploadPath = $"{uploadDirectory}/{nextBatchReadyToPrint.CertificatesFileName}";
                        await _externalFileTransferClient.UploadFile(fileContents, uploadPath);

                        var archiveDirectory = _options.ArchiveDirectory;
                        var archivePath = $"{archiveDirectory}/{nextBatchReadyToPrint.CertificatesFileName}";
                        await _internalFileTransferClient.UploadFile(fileContents, archivePath);

                        nextBatchReadyToPrint.FileUploadEndTime = DateTime.UtcNow;

                        printStatusUpdateMessages = await _batchService.Update(nextBatchReadyToPrint);

                        await _notificationService.SendPrintRequest(
                            nextBatchReadyToPrint.BatchNumber,
                            nextBatchReadyToPrint.Certificates,
                            nextBatchReadyToPrint.CertificatesFileName);
                    }
                }

                await _scheduleService.Save(schedule);
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, "PrintRequestCommand - Unable to send print request");
                }
                finally
                {
                    if (schedule != null && schedule.Id != Guid.Empty)
                    {
                        await _scheduleService.Fail(schedule);
                    }
                }

                throw;
            }

            return printStatusUpdateMessages;
        }

        private string GetCertificatesFileName(int batchNumber, DateTime batchCreated)
        {
            return $"PrintBatch-{batchNumber.ToString().PadLeft(3, '0')}-{batchCreated.UtcToTimeZoneTime():ddMMyyHHmm}.json";
        }
    }
}
