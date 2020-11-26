using Microsoft.Azure.WebJobs;
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
using System.Text;
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

        private ICollector<string> _messageQueue;

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

        public async Task Execute(ICollector<string> messageQueue)
        {
            Schedule schedule = null;

            try
            {
                _logger.Log(LogLevel.Information, "PrintRequestCommand - Started");

                _messageQueue = messageQueue;

                schedule = await _scheduleService.Get();
                if (schedule == null)
                {
                    _logger.Log(LogLevel.Information, "There is no print schedule which allows printing at this time");
                    return;
                }

                await _scheduleService.Start(schedule);

                var nextPrintBatchNumber = await _batchService.BuildPrintBatchReadyToPrint(schedule.RunTime, _options.AddReadyToPrintChunkSize);
                if (nextPrintBatchNumber != null)
                {
                    var certificates = (await _batchService.GetCertificatesForBatchNumber(nextPrintBatchNumber.Value)).Sanitise(_logger);
                    if (certificates.Count > 0)
                    {
                        var batch = await _batchService.Get(nextPrintBatchNumber.Value);
                        batch.Status = CertificateStatus.SentToPrinter;
                        batch.BatchCreated = DateTime.UtcNow;
                        batch.CertificatesFileName = $"PrintBatch-{nextPrintBatchNumber.Value.ToString().PadLeft(3, '0')}-{batch.BatchCreated.UtcToTimeZoneTime():ddMMyyHHmm}.json";
                        batch.Certificates = certificates;

                        var printOutput = _printCreator.Create(batch.BatchNumber, batch.Certificates);
                        var fileContents = JsonConvert.SerializeObject(printOutput);

                        batch.NumberOfCertificates = printOutput.Batch.TotalCertificateCount;
                        batch.NumberOfCoverLetters = printOutput.Batch.PostalContactCount;

                        batch.FileUploadStartTime = DateTime.UtcNow;
                        var uploadDirectory = _options.Directory;
                        var uploadPath = $"{uploadDirectory}/{batch.CertificatesFileName}";
                        await _externalFileTransferClient.UploadFile(fileContents, uploadPath);

                        var uploadedFileNames = await _externalFileTransferClient.GetFileNames(uploadDirectory, false);

                        var archiveDirectory = _options.ArchiveDirectory;
                        var archivePath = $"{archiveDirectory}/{batch.CertificatesFileName}";
                        await _internalFileTransferClient.UploadFile(fileContents, archivePath);

                        batch.FileUploadEndTime = DateTime.UtcNow;

                        LogUploadedFiles(uploadedFileNames, uploadDirectory);

                        await _batchService.Update(batch, _messageQueue, _options.PrintStatusUpdateChunkSize);

                        await _notificationService.SendPrintRequest(batch.BatchNumber, batch.Certificates, batch.CertificatesFileName);
                    }
                }

                await _scheduleService.Save(schedule);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Function Errored Message:: {e.Message} InnerException :: {e.InnerException} ", e);

                if (schedule != null && schedule.Id != Guid.Empty)
                {
                    await _scheduleService.Fail(schedule);
                }
                throw;
            }
        }

        private void LogUploadedFiles(List<string> fileNames, string directory)
        {
            var fileDetails = new StringBuilder();
            foreach (var file in fileNames)
            {
                fileDetails.Append(file + "\r\n");
            }

            if (fileDetails.Length > 0)
            {
                _logger.Log(LogLevel.Information, $"Uploaded Files to {directory} Contains\r\n{fileDetails}");
            }
        }
    }
}
