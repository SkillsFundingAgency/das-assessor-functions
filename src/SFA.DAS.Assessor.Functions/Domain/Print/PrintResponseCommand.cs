using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Exceptions;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Collections.Generic;
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
        private static readonly string DateTimePatternFormat = "ddMMyyHHmm";
        private static readonly string DateTimePatternRegEx = "[0-9]{10}";
        private static readonly string FilePatternRegEx = $@"^[Pp][Rr][Ii][Nn][Tt][Bb][Aa][Tt][Cc][Hh][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{{1,6}}-{DateTimePatternRegEx}.json";

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

        public async Task<List<string>> Execute()
        {
            List<string> printStatusUpdateMessages = new List<string>();

            _logger.Log(LogLevel.Information, "PrintResponseCommand - Started");

            var fileNames = await _externalFileTransferClient.GetFileNames(_options.Directory, FilePatternRegEx, false);
            if (!fileNames.Any())
            {
                _logger.Log(LogLevel.Information, "There are no certificate print responses from the printer to process");
                return null;
            }

            var sortedFileNames = fileNames.ToList().SortByDateTimePattern(DateTimePatternRegEx, DateTimePatternFormat);
            foreach (var fileName in sortedFileNames)
            {
                var fileContents = await _externalFileTransferClient.DownloadFile($"{_options.Directory}/{fileName}");
                var fileInfo = new PrintFileInfo(fileContents, fileName);

                try
                {
                    try
                    {
                        var batch = await ProcessFile(fileInfo);
                        var messages = await _batchService.Update(batch);
                        
                        await ArchiveFile(fileContents, fileName, _options.Directory, _options.ArchiveDirectory);
                        
                        printStatusUpdateMessages.AddRange(messages);
                    }
                    catch (FileFormatValidationException ex)
                    {
                        fileInfo.ValidationMessages.Add(ex.Message);
                        await CreateErrorFile(fileInfo, _options.Directory, _options.ErrorDirectory);
                        throw;
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Could not process print response file [{fileName}]");
                }
            }

            return printStatusUpdateMessages;
        }

        private async Task<Batch> ProcessFile(PrintFileInfo fileInfo)
        {
            var receipt = JsonConvert.DeserializeObject<PrintReceipt>(fileInfo.FileContent);

            if (receipt?.Batch == null || receipt.Batch.BatchDate == DateTime.MinValue)
            {
                fileInfo.InvalidFileContent = fileInfo.FileContent;
                throw new FileFormatValidationException($"Could not process print response file [{fileInfo.FileName}] due to invalid file format");
            }

            var batch = await _batchService.Get(receipt.Batch.BatchNumber);

            if (batch == null)
            {
                fileInfo.InvalidFileContent = fileInfo.FileContent;
                throw new FileFormatValidationException($"Could not process print response file [{fileInfo.FileName}] due to non matching Batch Number [{receipt.Batch.BatchNumber}]");
            }

            batch.BatchCreated = receipt.Batch.BatchDate;
            batch.NumberOfCoverLetters = receipt.Batch.PostalContactCount;
            batch.NumberOfCertificates = receipt.Batch.TotalCertificateCount;
            batch.PrintedDate = receipt.Batch.ProcessedDate;
            batch.DateOfResponse = DateTime.UtcNow;
            batch.Status = CertificateStatus.Printed;
            batch.Certificates = await _batchService.GetCertificatesForBatchNumber(batch.BatchNumber);

            return batch;
        }
    }
}
