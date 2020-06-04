using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly IBatchClient _batchClient;
        private readonly ICertificateClient _certificateClient;
        private readonly IScheduleClient _scheduleClient;
        
        private readonly INotificationService _notificationService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly SftpSettings _sftpSettings;

        public PrintProcessCommand(ILogger<PrintProcessCommand> logger,
            IPrintingJsonCreator printingJsonCreator,
            IPrintingSpreadsheetCreator printingSpreadsheetCreator,
            IBatchClient batchClient,
            ICertificateClient certificateClient,
            IScheduleClient scheduleClient,
            INotificationService notificationService,
            IFileTransferClient fileTransferClient,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _printingJsonCreator = printingJsonCreator;
            _printingSpreadsheetCreator = printingSpreadsheetCreator;
            _certificateClient = certificateClient;
            _batchClient = batchClient;
            _scheduleClient = scheduleClient;
            _notificationService = notificationService;
            _fileTransferClient = fileTransferClient;
            _sftpSettings = options?.Value;
        }

        public async Task Execute()
        {
            try
            {
                _logger.Log(LogLevel.Information, "Print Process Function Started");

                var schedule = await _scheduleClient.Get();
                if (schedule == null)
                {
                    _logger.Log(LogLevel.Information, "Print Function not scheduled to run at this time.");
                    return;
                }

                var batchNumber = await _batchClient.NextBatchId();
                var certificates = (await _certificateClient.Get(Interfaces.CertificateStatus.ToBePrinted)).ToList().Sanitise(_logger);

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
                        Status = "Sent to Printer",
                        FileUploadStartTime = DateTime.UtcNow,
                        Period = DateTime.UtcNow.UtcToTimeZoneTime().ToString("MMyy"),
                        BatchCreated = DateTime.UtcNow,
                        ScheduledDate = schedule.RunTime,
                        Certificates = certificates
                    };

                    if (_sftpSettings.UseJson)
                    {
                        uploadDirectory = _sftpSettings.PrintRequestDirectory;
                        batch.CertificatesFileName = $"PrintRequest-{batchNumber.ToString().PadLeft(3, '0')}-{DateTime.UtcNow.UtcToTimeZoneTime():ddMMyyHHmm}.json";
                        _printingJsonCreator.Create(batchNumber, certificates, $"{uploadDirectory}/{batch.CertificatesFileName}");
                    }
                    else
                    {
                        uploadDirectory = _sftpSettings.UploadDirectory;
                        batch.CertificatesFileName = $"IFA-Certificate-{DateTime.UtcNow.UtcToTimeZoneTime():MMyy}-{batchNumber.ToString().PadLeft(3, '0')}.xlsx";
                        _printingSpreadsheetCreator.Create(batchNumber, certificates, $"{uploadDirectory}/{batch.CertificatesFileName}");
                    }

                    await _notificationService.Send(batchNumber, certificates, batch.CertificatesFileName);
                    uploadedFileNames = await _fileTransferClient.GetFileNames(uploadDirectory);

                    batch.FileUploadEndTime = DateTime.UtcNow;
                    batch.NumberOfCertificates = certificates.Count;
                    batch.NumberOfCoverLetters = 0;

                    LogUploadedFiles(uploadedFileNames, uploadDirectory);
                    await _batchClient.Save(batch);
                }
                await _scheduleClient.Save(schedule);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, "Function Errored", e);
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
