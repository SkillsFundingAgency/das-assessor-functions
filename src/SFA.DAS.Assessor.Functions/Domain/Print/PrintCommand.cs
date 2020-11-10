using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintCommand : IPrintCommand
    {
        private readonly ILogger<PrintCommand> _logger;
        private readonly IPrintCreator _printCreator;
        private readonly IBatchService _batchService;
        private readonly IScheduleService _scheduleService;
        
        private readonly INotificationService _notificationService;
        private readonly IFileTransferClient _externalFileTransferClient;
        private readonly IFileTransferClient _internalFileTransferClient;
        private readonly CertificatePrintFunctionSettings _settings;

        private ICollector<string> _messageQueue;

        public PrintCommand(
            ILogger<PrintCommand> logger,
            IPrintCreator printCreator,
            IBatchService batchService,
            IScheduleService scheduleService,
            INotificationService notificationService,
            IFileTransferClient externalFileTransferClient,
            IFileTransferClient internalFileTransferClient,
            IOptions<CertificatePrintFunctionSettings> options)
        {
            _logger = logger;
            _printCreator = printCreator;
            _batchService = batchService;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _externalFileTransferClient = externalFileTransferClient;
            _internalFileTransferClient = internalFileTransferClient;
            _settings = options?.Value;

            _externalFileTransferClient.ContainerName = _settings.PrintRequestExternalBlobContainer;
            _internalFileTransferClient.ContainerName = _settings.PrintRequestInternalBlobContainer;
        }

        public async Task Execute(ICollector<string> messageQueue)
        {
            try
            {
                _logger.Log(LogLevel.Information, "Print command started");

                _messageQueue = messageQueue;

                var schedule = await _scheduleService.Get();
                if (schedule == null)
                {
                    _logger.Log(LogLevel.Information, "There is no print schedule which allows printing at this time");
                    return;
                }

                var nextPrintBatchNumber = await _batchService.BuildPrintBatchReadyToPrint(schedule.RunTime, _settings.AddReadyToPrintChunkSize);
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
                        var uploadDirectory = _settings.PrintRequestDirectory;
                        var uploadPath = $"{uploadDirectory}/{batch.CertificatesFileName}";
                        await _externalFileTransferClient.UploadFile(fileContents, uploadPath);

                        var uploadedFileNames = await _externalFileTransferClient.GetFileNames(uploadDirectory, false);

                        var archiveDirectory = _settings.ArchivePrintRequestDirectory;
                        var archivePath = $"{archiveDirectory}/{batch.CertificatesFileName}";
                        await _internalFileTransferClient.UploadFile(fileContents, archivePath);

                        batch.FileUploadEndTime = DateTime.UtcNow;

                        LogUploadedFiles(uploadedFileNames, uploadDirectory);

                        await _batchService.Update(batch, _messageQueue, _settings.PrintStatusUpdateChunkSize);

                        await _notificationService.Send(batch.Certificates.Count, batch.CertificatesFileName);
                    }

                    await _scheduleService.Save(schedule);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Function Errored Message:: {e.Message} InnerException :: {e.InnerException} ", e);
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
