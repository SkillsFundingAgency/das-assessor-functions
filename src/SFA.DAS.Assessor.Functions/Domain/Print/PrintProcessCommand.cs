using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class PrintProcessCommand : IPrintProcessCommand
    {
        private readonly ILogger<PrintProcessCommand> _logger;
        private readonly IPrintingSpreadsheetCreator _printingSpreadsheetCreator;
        private readonly IPrintingJsonCreator _printingJsonCreator;
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        private readonly INotificationService _notificationService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly IOptions<SftpSettings> _options;

        public PrintProcessCommand(ILogger<PrintProcessCommand> logger,
            IPrintingJsonCreator printingJsonCreator,
            IPrintingSpreadsheetCreator printingSpreadsheetCreator,
            IAssessorServiceApiClient assessorServiceApi,
            INotificationService notificationService,
            IFileTransferClient fileTransferClient,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _printingJsonCreator = printingJsonCreator;
            _printingSpreadsheetCreator = printingSpreadsheetCreator;
            _assessorServiceApi = assessorServiceApi;
            _notificationService = notificationService;
            _fileTransferClient = fileTransferClient;
            _options = options;
        }

        public async Task Execute()
        {
            await UploadCertificateDetailsToPinter();
            await DownloadAndDeleteCertificatePrinterResponses();
        }

        private async Task DownloadAndDeleteCertificatePrinterResponses()
        {
            var fileList = await _fileTransferClient.GetListOfDownloadedFiles();

            // printResponse-MMYY-XXXXXX.json where XXX = 001, 002... 999999 etc
            const string pattern = @"^[Pp][Rr][Ii][Nn][Tt][Rr][Ee][Ss][Pp][Oo][Nn][Ss][Ee]-[0-9]{4}-[0-9]{1,6}.json";

            var certificateResponseFiles = fileList.Where(f => Regex.IsMatch(f, pattern));
            var filesToProcesses = certificateResponseFiles as string[] ?? certificateResponseFiles.ToArray();
            if (!filesToProcesses.Any())
            {
                _logger.Log(LogLevel.Information, "No certificate responses to process");
                return;
            }

            foreach (var fileToProcess in filesToProcesses)
            {
                await ProcessEachFileToUploadThenDelete(fileToProcess);
            }
        }

        private async Task ProcessEachFileToUploadThenDelete(string fileToProcess)
        {
            var stringBatchResponse = _fileTransferClient.DownloadFile(fileToProcess);
            var batchResponse = JsonConvert.DeserializeObject<BatchResponse>(stringBatchResponse);

            if (batchResponse?.Batch == null || batchResponse.Batch.BatchDate == DateTime.MinValue)
            {
                _logger.Log(LogLevel.Information, $"Could not process downloaded file to correct format [{fileToProcess}]");
                return;
            }

            batchResponse.Batch.DateOfResponse = DateTime.UtcNow;
            var batchNumber = batchResponse.Batch.BatchNumber;

            var batchLogResponse = await _assessorServiceApi.GetGetBatchLogByBatchNumber(batchNumber);

            if (batchLogResponse?.Id == null)
            {
                _logger.Log(LogLevel.Information, $"Could not match an existing batch Log Batch Number [{batchNumber}]");
                return;
            }

            if (!int.TryParse(batchNumber, out int batchNumberToInt))
            {
                _logger.Log(LogLevel.Information, $"The Batch Number is not an integer [{batchNumber}]");
                return;
            }

            var batch = new BatchData
            {
                BatchNumber = batchNumberToInt,
                BatchDate = batchResponse.Batch.BatchDate,
                PostalContactCount = batchResponse.Batch.PostalContactCount,
                TotalCertificateCount = batchResponse.Batch.TotalCertificateCount,
                PrintedDate = batchResponse.Batch.PrintedDate,
                PostedDate = batchResponse.Batch.PostedDate,
                DateOfResponse = batchResponse.Batch.DateOfResponse
            };

            await _assessorServiceApi.UpdateBatchDataInBatchLog((Guid)batchLogResponse.Id, batch);
            _fileTransferClient.DeleteFile(fileToProcess);
        }

        private async Task UploadCertificateDetailsToPinter()
        {
            try
            {
                _logger.Log(LogLevel.Information, "Print Process Function Started");

                var scheduleRun = await _assessorServiceApi.GetSchedule(ScheduleType.PrintRun);
                if (scheduleRun == null)
                {
                    _logger.Log(LogLevel.Information, "Print Function not scheduled to run at this time.");
                    return;
                }

                var batchLogResponse = await _assessorServiceApi.GetCurrentBatchLog();

                var batchNumber = batchLogResponse.BatchNumber + 1;
                var certificates = (await _assessorServiceApi.GetCertificatesToBePrinted()).ToList().Sanitise(_logger);

                if (certificates.Count == 0)
                {
                    _logger.Log(LogLevel.Information, "No certificates to process");
                }
                else
                {
                    var certificateFileName =
                        $"IFA-Certificate-{DateTime.UtcNow.UtcToTimeZoneTime():MMyy}-{batchNumber.ToString().PadLeft(3, '0')}.json";
                    var excelFileName = $"IFA-Certificate-{DateTime.UtcNow.UtcToTimeZoneTime()}-{batchNumber.ToString().PadLeft(3, '0')}.xlsx";

                    var batchLogRequest = new CreateBatchLogRequest
                    {
                        BatchNumber = batchNumber,
                        FileUploadStartTime = DateTime.UtcNow,
                        Period = DateTime.UtcNow.UtcToTimeZoneTime().ToString("MMyy"),
                        BatchCreated = DateTime.UtcNow,
                        CertificatesFileName = certificateFileName
                    };

                    if (_options.Value.UseJson)
                    {
                        _printingJsonCreator.Create(batchNumber, certificates, certificateFileName);
                        await _notificationService.Send(batchNumber, certificates, certificateFileName);
                    }
                    else
                    {
                        _printingSpreadsheetCreator.Create(batchNumber, certificates);
                        await _notificationService.Send(batchNumber, certificates, excelFileName);
                    }

                    batchLogRequest.FileUploadEndTime = DateTime.UtcNow;
                    batchLogRequest.NumberOfCertificates = certificates.Count;
                    batchLogRequest.NumberOfCoverLetters = 0;
                    batchLogRequest.ScheduledDate = batchLogResponse.ScheduledDate;

                    await _fileTransferClient.LogUploadDirectory();
                    await _assessorServiceApi.CreateBatchLog(batchLogRequest);
                    await _assessorServiceApi.ChangeStatusToPrinted(batchNumber, certificates);
                }
                await _assessorServiceApi.CompleteSchedule(scheduleRun.Id);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, "Function Errored", e);
                throw;
            }
        }
    }
}
