﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Exceptions;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintResponseCommand : NotificationCommand, IPrintResponseCommand
    {
        private readonly ILogger<PrintResponseCommand> _logger;
        private readonly IBatchService _batchService;
        private readonly PrintResponseOptions _options;

        // PrintBatchResponse-XXXXXX-ddMMyyHHmm.json where XXX = 001, 002... 999999 etc                
        private static readonly string DateTimePattern = "[0-9]{10}";
        private static readonly string FilePattern = $@"^[Pp][Rr][Ii][Nn][Tt][Bb][Aa][Tt][Cc][Hh][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{{1,6}}-{DateTimePattern}.json";

        public PrintResponseCommand(
            ILogger<PrintResponseCommand> logger,
            IBatchService batchService,
            IExternalBlobFileTransferClient externalFileTransferClient,
            IInternalBlobFileTransferClient internalFileTransferClient,
            IOptions<PrintResponseOptions> printReponseOptions)
            : base(externalFileTransferClient, internalFileTransferClient)
        {
            _logger = logger;
            _batchService = batchService;
            _options = printReponseOptions?.Value;
        }

        public async Task Execute()
        {
            _logger.Log(LogLevel.Information, "Print Response Notification Function Started");
            await Process(_options.Directory, FilePattern, DateTimePattern, "ddMMyyHHmm");
        }

        private async Task Process(string downloadDirectoryName, string filePattern, string dateTimePattern, string dateTimeFormat)
        {
            var fileNames = await _externalFileTransferClient.GetFileNames(downloadDirectoryName, filePattern, false);
            if (!fileNames.Any())
            {
                _logger.Log(LogLevel.Information, "There are no certificate print notifications from the printer to process");
                return;
            }

            var sortedFileNames = fileNames.ToList().SortByDateTimePattern(dateTimePattern, dateTimeFormat);
            foreach (var fileName in sortedFileNames)
            {
                try
                {
                    var fileContents = await _externalFileTransferClient.DownloadFile($"{downloadDirectoryName}/{fileName}");
                    var fileInfo = new PrintNotificationFileInfo(fileContents, fileName);

                    var batch = await ProcessFile(fileInfo);
                    await _batchService.Save(batch);

                    await ArchiveFile(fileContents, fileName, downloadDirectoryName, _options.ArchiveDirectory);
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