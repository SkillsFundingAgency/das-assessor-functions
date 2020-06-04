using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Linq;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintNotificationCommand : IPrintNotificationCommand
    {
        private readonly ILogger<PrintNotificationCommand> _logger;
        private readonly IBatchClient _batchClient;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly SftpSettings _sftpSettings;

        public PrintNotificationCommand(ILogger<PrintNotificationCommand> logger,
            IBatchClient batchClient,
            IFileTransferClient fileTransferClient,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _batchClient = batchClient;
            _fileTransferClient = fileTransferClient;
            _sftpSettings = options?.Value;
        }

        public async Task Execute()
        {
            string directoryName;
            string filePattern;
            if (_sftpSettings.UseJson)
            {
                directoryName = _sftpSettings.PrintResponseDirectory;
                // PrintResponse-XXXXXX-ddMMyyHHmm.json where XXX = 001, 002... 999999 etc                
                filePattern = @"^[Pp][Rr][Ii][Nn][Tt][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{1,6}-[0-9]{10}.json";
            }
            else
            {
                directoryName = _sftpSettings.ProofDirectory;
                // printResponse-MMYY-XXXXXX.json where XXX = 001, 002... 999999 etc
                filePattern = @"^[Pp][Rr][Ii][Nn][Tt][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{4}-[0-9]{1,6}.json";
            }
            
            var fileNames = await _fileTransferClient.GetFileNames(directoryName, filePattern);

            if (!fileNames.Any())
            {
                _logger.Log(LogLevel.Information, "There are no certificate print notifications from the printer to process");
                return;
            }

            foreach (var fileName in fileNames)
            {
                if (_sftpSettings.UseJson)
                {
                    await ProcessFile(fileName);
                }
                else
                {
                    await ProcessLegacyFile(fileName);
                }
            }
        }

        private async Task ProcessFile(string fileName)
        {
            var printNotification = JsonConvert.DeserializeObject<PrintNotification>(_fileTransferClient.DownloadFile($"{_sftpSettings.PrintResponseDirectory}/{fileName}"));

            if (printNotification?.Batch == null || printNotification.Batch.BatchDate == DateTime.MinValue)
            {
                _logger.Log(LogLevel.Information, $"Could not process print notifications due to invalid file format [{fileName}]");
                return;
            }

            if (!int.TryParse(printNotification.Batch.BatchNumber, out int batchNumberToInt))
            {
                _logger.Log(LogLevel.Information, $"Could not process print notifications the Batch Number is not an integer [{printNotification.Batch.BatchNumber}]");
                return;
            }

            var batch = await _batchClient.Get(batchNumberToInt);

            if (batch == null)
            {
                _logger.Log(LogLevel.Information, $"Could not process print notifications unable to match an existing batch Log Batch Number [{batchNumberToInt}] in the print notification in the file [{fileName}]");
                return;
            }

            batch.BatchCreated = printNotification.Batch.BatchDate;
            batch.NumberOfCoverLetters = printNotification.Batch.PostalContactCount;
            batch.NumberOfCertificates = printNotification.Batch.TotalCertificateCount;
            batch.PrintedDate = printNotification.Batch.ProcessedDate;
            batch.DateOfResponse = DateTime.UtcNow;
            batch.Status = "Printed";

            await _batchClient.Save(batch);

            _fileTransferClient.DeleteFile($"{_sftpSettings.PrintResponseDirectory}/{fileName}");
        }

        private async Task ProcessLegacyFile(string fileName)
        {
            var stringBatchResponse = _fileTransferClient.DownloadFile($"{_sftpSettings.ProofDirectory}/{fileName}");

            var batchResponse = JsonConvert.DeserializeObject<BatchResponse>(stringBatchResponse);

            if (batchResponse?.Batch == null || batchResponse.Batch.BatchDate == DateTime.MinValue)
            {
                _logger.Log(LogLevel.Information, $"Could not process downloaded file due to invalid file format [{fileName}]");
                return;
            }

            if (!int.TryParse(batchResponse.Batch.BatchNumber, out int batchNumberToInt))
            {
                _logger.Log(LogLevel.Information, $"The Batch Number is not an integer [{batchResponse.Batch.BatchNumber}]");
                return;
            }

            var batch = await _batchClient.Get(batchNumberToInt);

            if (batch == null)
            {
                _logger.Log(LogLevel.Information, $"Could not match an existing batch Log Batch Number [{batchNumberToInt}]");
                return;
            }

            batch.BatchCreated = batchResponse.Batch.BatchDate;
            batch.NumberOfCoverLetters = batchResponse.Batch.PostalContactCount;
            batch.NumberOfCertificates = batchResponse.Batch.TotalCertificateCount;
            batch.PrintedDate = batchResponse.Batch.PrintedDate;
            batch.PostedDate = batchResponse.Batch.PostedDate;
            batch.DateOfResponse = DateTime.UtcNow;
            batch.Status = "Printed";

            await _batchClient.Save(batch);

            _fileTransferClient.DeleteFile($"{_sftpSettings.ProofDirectory}/{fileName}");
        }
    }
}
