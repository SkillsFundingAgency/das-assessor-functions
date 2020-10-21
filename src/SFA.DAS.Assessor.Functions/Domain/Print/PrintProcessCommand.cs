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
    public class PrintProcessCommand : IPrintProcessCommand
    {
        private readonly ILogger<PrintProcessCommand> _logger;
        private readonly IPrintCreator _printCreator;
        private readonly IBatchService _batchService;
        private readonly ICertificateService _certificateService;
        private readonly IScheduleService _scheduleService;
        
        private readonly INotificationService _notificationService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly CertificatePrintFunctionSettings _settings;

        public PrintProcessCommand(
            ILogger<PrintProcessCommand> logger,
            IPrintCreator printCreator,
            IBatchService batchService,
            ICertificateService certificateService,
            IScheduleService scheduleService,
            INotificationService notificationService,
            IFileTransferClient fileTransferClient,
            IOptions<CertificatePrintFunctionSettings> options)
        {
            _logger = logger;
            _printCreator = printCreator;
            _certificateService = certificateService;
            _batchService = batchService;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _fileTransferClient = fileTransferClient;
            _settings = options?.Value;

            _fileTransferClient.ContainerName = _settings.PrintRequestBlobContainer;
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
                var certificates = (await _certificateService.Get(CertificateStatus.ToBePrinted)).ToList().Sanitise(_logger);

                if (certificates.Count == 0)
                {
                    _logger.Log(LogLevel.Information, "No certificates to process");
                }
                else
                {
                    var uploadedFileNames = new List<string>();

                    var batch = new Batch
                    {
                        BatchNumber = batchNumber,
                        Status = "SentToPrinter",
                        FileUploadStartTime = DateTime.UtcNow,
                        Period = DateTime.UtcNow.UtcToTimeZoneTime().ToString("MMyy"),
                        BatchCreated = DateTime.UtcNow,
                        ScheduledDate = schedule.RunTime,
                        CertificatesFileName = $"PrintBatch-{batchNumber.ToString().PadLeft(3, '0')}-{DateTime.UtcNow.UtcToTimeZoneTime():ddMMyyHHmm}.json",
                        Certificates = certificates
                    };

                    var uploadDirectory = _settings.PrintRequestDirectory;
                    var uploadPath = $"{uploadDirectory}/{batch.CertificatesFileName}";
                    var fileContents = _printCreator.Create(batch.BatchNumber, batch.Certificates, uploadPath);

                    await UploadPrintRequest(uploadPath, fileContents);
                    uploadedFileNames = await _fileTransferClient.GetFileNames(uploadDirectory, false);

                    await _notificationService.Send(batchNumber, certificates, batch.CertificatesFileName);
                    
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

        private async Task UploadPrintRequest(string path, string fileContents)
        {
            byte[] array = Encoding.ASCII.GetBytes(fileContents);
            using (var stream = new MemoryStream(array))
            {
                await _fileTransferClient.UploadFile(stream, path);
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
