using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Exceptions;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintNotificationCommand : IPrintNotificationCommand
    {
        private readonly ILogger<PrintNotificationCommand> _logger;
        private readonly IBatchService _batchService;
        private readonly IFileTransferClient _externalFileTransferClient;
        private readonly IFileTransferClient _internalFileTransferClient;
        private readonly CertificatePrintNotificationFunctionSettings _settings;

        // PrintBatchResponse-XXXXXX-ddMMyyHHmm.json where XXX = 001, 002... 999999 etc                
        private static readonly string DateTimePattern = "[0-9]{10}";
        private static readonly string FilePattern = $@"^[Pp][Rr][Ii][Nn][Tt][Bb][Aa][Tt][Cc][Hh][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{{1,6}}-{DateTimePattern}.json";

        public PrintNotificationCommand(
            ILogger<PrintNotificationCommand> logger,
            IBatchService batchService,
            IFileTransferClient externalFileTransferClient,
            IFileTransferClient internalFileTransferClient,
            IOptions<CertificatePrintNotificationFunctionSettings> options)
        {
            _logger = logger;
            _batchService = batchService;
            _externalFileTransferClient = externalFileTransferClient;
            _internalFileTransferClient = internalFileTransferClient;
            _settings = options?.Value;

            _externalFileTransferClient.ContainerName = _settings.PrintResponseExternalBlobContainer;
            _internalFileTransferClient.ContainerName = _settings.PrintResponseInternalBlobContainer;
        }

        public async Task Execute()
        {
            _logger.Log(LogLevel.Information, "Print Response Notification Function Started");
            await Process(_settings.PrintResponseDirectory, FilePattern, DateTimePattern, "ddMMyyHHmm", (f) => ProcessFile(f));
        }

        private async Task Process(string directoryName, string filePattern, string dateTimePattern, string dateTimeFormat, Func<PrintNotificationFileInfo, Task<Batch>> processFile)
        {
            var fileNames = await _externalFileTransferClient.GetFileNames(directoryName, filePattern, false);
            if (!fileNames.Any())
            {
                _logger.Log(LogLevel.Information, "There are no certificate print notifications from the printer to process");
                return;
            }

            var sortedFileNames = fileNames.ToList().SortByDateTimePattern(dateTimePattern, dateTimeFormat);
            foreach (var fileName in sortedFileNames)
            {
                var fileContent = await _externalFileTransferClient.DownloadFile($"{directoryName}/{fileName}");
                var fileInfo = new PrintNotificationFileInfo(fileContent, fileName);

                try
                {
                    var batch = await processFile(fileInfo);
                    await _batchService.Save(batch);
                    await _externalFileTransferClient.MoveFile(
                        $"{directoryName}/{fileName}",
                        _internalFileTransferClient,
                        $"{_settings.ArchivePrintResponseDirectory}/{fileName}");
                }
                catch (FileFormatValidationException ex)
                {
                    _logger.Log(LogLevel.Information, ex.Message);
                }
            }
        }

        private async Task<Batch> ProcessFile(PrintNotificationFileInfo file)
        {
            var receipt = JsonConvert.DeserializeObject<PrintReceipt>(file.FileContent);

            if (receipt?.Batch == null || receipt.Batch.BatchDate == DateTime.MinValue)
            {
                throw new FileFormatValidationException($"Could not process print notifications due to invalid file format [{file.FileName}]");
            }

            var batch = await _batchService.Get(receipt.Batch.BatchNumber);

            if (batch == null)
            {
                throw new FileFormatValidationException($"Could not process print notifications unable to match an existing batch Log Batch Number [{receipt.Batch.BatchNumber}] in the print notification in the file [{file.FileName}]");
            }

            batch.BatchCreated = receipt.Batch.BatchDate;
            batch.NumberOfCoverLetters = receipt.Batch.PostalContactCount;
            batch.NumberOfCertificates = receipt.Batch.TotalCertificateCount;
            batch.PrintedDate = receipt.Batch.ProcessedDate;
            batch.DateOfResponse = DateTime.UtcNow;
            batch.Status = "Printed";

            return batch;
        }
    }
}
