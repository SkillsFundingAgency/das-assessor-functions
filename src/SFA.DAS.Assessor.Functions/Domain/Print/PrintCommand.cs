using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintCommand : IPrintCommand
    {
        private readonly ILogger<PrintCommand> _logger;
        private readonly IPrintCreator _printCreator;
        private readonly IBatchService _batchService;
        private readonly ICertificateService _certificateService;
        private readonly IScheduleService _scheduleService;
        
        private readonly INotificationService _notificationService;
        private readonly IFileTransferClient _externalFileTransferClient;
        private readonly IFileTransferClient _internalFileTransferClient;
        private readonly CertificatePrintFunctionSettings _settings;

        public PrintCommand(
            ILogger<PrintCommand> logger,
            IPrintCreator printCreator,
            IBatchService batchService,
            ICertificateService certificateService,
            IScheduleService scheduleService,
            INotificationService notificationService,
            IFileTransferClient externalFileTransferClient,
            IFileTransferClient internalFileTransferClient,
            IOptions<CertificatePrintFunctionSettings> options)
        {
            _logger = logger;
            _printCreator = printCreator;
            _certificateService = certificateService;
            _batchService = batchService;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _externalFileTransferClient = externalFileTransferClient;
            _internalFileTransferClient = internalFileTransferClient;
            _settings = options?.Value;

            _externalFileTransferClient.ContainerName = _settings.PrintRequestExternalBlobContainer;
            _internalFileTransferClient.ContainerName = _settings.PrintRequestInternalBlobContainer;
        }

        public ICollector<string> StorageQueue { get; set; }

        public async Task Execute()
        {
            try
            {
                _logger.Log(LogLevel.Information, "Print command started");

                var schedule = await _scheduleService.Get();
                if (schedule == null)
                {
                    _logger.Log(LogLevel.Information, "There is no print schedule which allows printing at this time");
                    return;
                }

                var nextBatchNumber = await _batchService.CreateNextBatchToBePrinted(schedule.RunTime);
                while (nextBatchNumber != null)
                {
                    var certificates = (await _batchService.GetCertificatesToBePrinted(nextBatchNumber.Value)).Sanitise(_logger);

                    if (certificates.Count == 0)
                    {
                        _logger.Log(LogLevel.Information, "No certificates to process");
                    }
                    else
                    {
                        var batch = await _batchService.Get(nextBatchNumber.Value);
                        
                        batch.Status = "SentToPrinter";
                        batch.FileUploadStartTime = DateTime.UtcNow;
                        batch.Period = DateTime.UtcNow.UtcToTimeZoneTime().ToString("MMyy");
                        batch.CertificatesFileName = $"PrintBatch-{nextBatchNumber.Value.ToString().PadLeft(3, '0')}-{DateTime.UtcNow.UtcToTimeZoneTime():ddMMyyHHmm}.json";
                        batch.Certificates = certificates;
                        
                        var fileContents = _printCreator.Create(batch.BatchNumber, batch.Certificates);

                        var uploadDirectory = _settings.PrintRequestDirectory;
                        var uploadPath = $"{uploadDirectory}/{batch.CertificatesFileName}";
                        await _externalFileTransferClient.UploadFile(fileContents, uploadPath);

                        var uploadedFileNames = await _externalFileTransferClient.GetFileNames(uploadDirectory, false);

                        var archiveDirectory = _settings.ArchivePrintRequestDirectory;
                        var archivePath = $"{archiveDirectory}/{batch.CertificatesFileName}";
                        await _internalFileTransferClient.UploadFile(fileContents, archivePath);

                        batch.FileUploadEndTime = DateTime.UtcNow;
                        batch.NumberOfCertificates = certificates.Count;
                        batch.NumberOfCoverLetters = 0;

                        LogUploadedFiles(uploadedFileNames, uploadDirectory);

                        await _batchService.Update(batch, StorageQueue);

                        await _notificationService.Send(batch.Certificates.Count, batch.CertificatesFileName);
                    }
                    
                    nextBatchNumber = await _batchService.GetNextBatchNumberToBePrinted();
                }
                
                await _scheduleService.Save(schedule);
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
