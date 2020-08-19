using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintProcessCommand : IPrintProcessCommand
    {
        private readonly ILogger<PrintProcessCommand> _logger;
        private readonly IPrintingSpreadsheetCreator _printingSpreadsheetCreator;
        private readonly IPrintingJsonCreator _printingJsonCreator;
        private readonly IBatchService _batchService;
        private readonly ICertificateService _certificateService;
        private readonly IScheduleService _scheduleService;
        
        private readonly INotificationService _notificationService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly SftpSettings _sftpSettings;

        public PrintProcessCommand(
            ILogger<PrintProcessCommand> logger,
            IPrintingJsonCreator printingJsonCreator,
            IPrintingSpreadsheetCreator printingSpreadsheetCreator,
            IBatchService batchService,
            ICertificateService certificateService,
            IScheduleService scheduleService,
            INotificationService notificationService,
            IFileTransferClient fileTransferClient,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _printingJsonCreator = printingJsonCreator;
            _printingSpreadsheetCreator = printingSpreadsheetCreator;
            _certificateService = certificateService;
            _batchService = batchService;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _fileTransferClient = fileTransferClient;
            _sftpSettings = options?.Value;
        }

        public async Task Execute()
        {
            try
            {
                _logger.Log(LogLevel.Information, "Print Process Function Started");

                var schedule = await _scheduleService.Get();
                if (schedule == null)
                {
                    _logger.Log(LogLevel.Information, "Print Function not scheduled to run at this time.");
                    return;
                }

                var batchNumber = await _batchService.NextBatchId();
                _logger.Log(LogLevel.Information, $"BatchNumber : {batchNumber}");
                var certificates = (await _certificateService.Get(CertificateStatus.ToBePrinted)).ToList().Sanitise(_logger);

                if (certificates.Count == 0)
                {
                    _logger.Log(LogLevel.Information, "No certificates to process");
                }
                else
                {
                    var uploadedFileNames = new List<string>();
                    string uploadDirectory = "";

                    var batch = new Batch
                    {
                        BatchNumber = batchNumber,
                        Status = "SentToPrinter",
                        FileUploadStartTime = DateTime.UtcNow,
                        Period = DateTime.UtcNow.UtcToTimeZoneTime().ToString("MMyy"),
                        BatchCreated = DateTime.UtcNow,
                        ScheduledDate = schedule.RunTime,
                        Certificates = certificates
                    };

                    if (_sftpSettings.UseJson)
                    {
                        uploadDirectory = _sftpSettings.PrintRequestDirectory;
                        batch.CertificatesFileName = $"PrintBatch-{batchNumber.ToString().PadLeft(3, '0')}-{DateTime.UtcNow.UtcToTimeZoneTime():ddMMyyHHmm}.json";
                        _printingJsonCreator.Create(batchNumber, certificates, $"{uploadDirectory}/{batch.CertificatesFileName}");
                    }
                    else
                    {
                        uploadDirectory = _sftpSettings.UploadDirectory;
                        batch.CertificatesFileName = $"IFA-Certificate-{DateTime.UtcNow.UtcToTimeZoneTime():MMyy}-{batchNumber.ToString().PadLeft(3, '0')}.xlsx";
                        _printingSpreadsheetCreator.Create(batchNumber, certificates, $"{uploadDirectory}/{batch.CertificatesFileName}");
                    }

                    _logger.Log(LogLevel.Information, "Calling Notification Service");
                    await _notificationService.Send(batchNumber, certificates, batch.CertificatesFileName);
                    uploadedFileNames = await _fileTransferClient.GetFileNames(uploadDirectory);

                    _logger.Log(LogLevel.Information, $"uploadedFileNames :: {uploadedFileNames.Count()}");

                    batch.FileUploadEndTime = DateTime.UtcNow;
                    batch.NumberOfCertificates = certificates.Count;
                    batch.NumberOfCoverLetters = 0;
                    batch.ScheduledDate = schedule.RunTime;

                    LogUploadedFiles(uploadedFileNames, uploadDirectory);
                    await _batchService.Save(batch);
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
