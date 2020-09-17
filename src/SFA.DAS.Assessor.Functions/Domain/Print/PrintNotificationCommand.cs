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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintNotificationCommand : IPrintNotificationCommand
    {
        private readonly ILogger<PrintNotificationCommand> _logger;
        private readonly IBatchService _batchService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly SftpSettings _sftpSettings;

        // PrintBatchResponse-XXXXXX-ddMMyyHHmm.json where XXX = 001, 002... 999999 etc                
        private static readonly string DateTimePattern = "[0-9]{10}";
        private static readonly string FilePattern = $@"^[Pp][Rr][Ii][Nn][Tt][Bb][Aa][Tt][Cc][Hh][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{{1,6}}-{DateTimePattern}.json";

        // printResponse-MMYY-XXXXXX.json where XXX = 001, 002... 999999 etc
        private static readonly string LegacyDateTimePattern = "[0-9]{4}";
        private static readonly string LegacyFilePattern = $@"^[Pp][Rr][Ii][Nn][Tt][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-{LegacyDateTimePattern}-[0-9]{{1,6}}.json";

        public PrintNotificationCommand(
            ILogger<PrintNotificationCommand> logger,
            IBatchService batchService,
            IFileTransferClient fileTransferClient,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _batchService = batchService;
            _fileTransferClient = fileTransferClient;
            _sftpSettings = options?.Value;
        }

        public async Task Execute()
        {
            _logger.Log(LogLevel.Information, "Print Response Notification Function Started");

            if (_sftpSettings.UseJson)
            {
                await Process(_sftpSettings.PrintResponseDirectory, FilePattern, DateTimePattern, "ddMMyyHHmm", (f) => ProcessFile(f));
            }
            else
            {
                await Process(_sftpSettings.ProofDirectory, LegacyFilePattern, LegacyDateTimePattern, "MMYY", (f) => ProcessLegacyFile(f));
            }
        }

        private async Task Process(string directoryName, string filePattern, string dateTimePattern, string dateTimeFormat, Func<PrintNotificationFileInfo, Task<Batch>> processFile)
        {
            var fileNames = await _fileTransferClient.GetFileNames(directoryName, filePattern);
            if (!fileNames.Any())
            {
                _logger.Log(LogLevel.Information, "There are no certificate print notifications from the printer to process");
                return;
            }

            var sortedFileNames = fileNames.ToList().SortByDateTimePattern(dateTimePattern, dateTimeFormat);
            foreach (var fileName in sortedFileNames)
            {
                var fileInfo = new PrintNotificationFileInfo(_fileTransferClient.DownloadFile($"{directoryName}/{fileName}"), fileName);

                try
                {
                    var batch = await processFile(fileInfo);
                    await _batchService.Save(batch);
                    _fileTransferClient.MoveFile($"{directoryName}/{fileName}", _sftpSettings.ArchivePrintResponseDirectory);
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

            if (!int.TryParse(receipt.Batch.BatchNumber, out int batchNumberToInt))
            {
                throw new FileFormatValidationException($"Could not process print notifications the Batch Number is not an integer [{receipt.Batch.BatchNumber}] in the print notification in the file [{file.FileName}]");
            }

            var batch = await _batchService.Get(batchNumberToInt);

            if (batch == null)
            {
                throw new FileFormatValidationException($"Could not process print notifications unable to match an existing batch Log Batch Number [{batchNumberToInt}] in the print notification in the file [{file.FileName}]");
            }

            batch.BatchCreated = receipt.Batch.BatchDate;
            batch.NumberOfCoverLetters = receipt.Batch.PostalContactCount;
            batch.NumberOfCertificates = receipt.Batch.TotalCertificateCount;
            batch.PrintedDate = receipt.Batch.ProcessedDate;
            batch.DateOfResponse = DateTime.UtcNow;
            batch.Status = "Printed";

            return batch;
        }

        private async Task<Batch> ProcessLegacyFile(PrintNotificationFileInfo file)
        {
            var batchResponse = JsonConvert.DeserializeObject<BatchResponse>(file.FileContent);

            if (batchResponse?.Batch == null || batchResponse.Batch.BatchDate == DateTime.MinValue)
            {
                throw new FileFormatValidationException($"Could not process downloaded file due to invalid file format [{file.FileName}]");
            }

            if (!int.TryParse(batchResponse.Batch.BatchNumber, out int batchNumberToInt))
            {
                throw new FileFormatValidationException($"The Batch Number is not an integer [{batchResponse.Batch.BatchNumber}] in the print notification in the file [{file.FileName}]");
            }

            var batch = await _batchService.Get(batchNumberToInt);

            if (batch == null)
            {
                throw new FileFormatValidationException($"Could not process print notifications unable to match an existing batch Log Batch Number [{batchNumberToInt}] in the print notification in the file [{file.FileName}]");
            }

            batch.BatchCreated = batchResponse.Batch.BatchDate;
            batch.NumberOfCoverLetters = batchResponse.Batch.PostalContactCount;
            batch.NumberOfCertificates = batchResponse.Batch.TotalCertificateCount;
            batch.PrintedDate = batchResponse.Batch.PrintedDate;
            batch.PostedDate = batchResponse.Batch.PostedDate;
            batch.DateOfResponse = DateTime.UtcNow;
            batch.Status = "Printed";

            return batch;
        }
    }
}
