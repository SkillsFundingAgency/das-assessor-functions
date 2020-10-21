using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class DeliveryNotificationCommand : IDeliveryNotificationCommand
    {
        // DeliveryNotifications-ddMMyyHHmm.json
        private static readonly string DateTimePattern = "[0-9]{10}";
        private static readonly string FilePattern = $@"^[Dd][Ee][Ll][Ii][Vv][Ee][Rr][Yy][Nn][Oo][Tt][Ii][Ff][Ii][Cc][Aa][Tt][Ii][Oo][Nn][Ss]-{DateTimePattern}.json";
        public const string Delivered = "Delivered";
        public const string NotDelivered = "NotDelivered";
        public static string[] DeliveryNotificationStatus = new[] { Delivered, NotDelivered };

        private readonly ILogger<DeliveryNotificationCommand> _logger;
        private readonly ICertificateService _certificateService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly SftpSettings _sftpSettings;

        public DeliveryNotificationCommand(
            ILogger<DeliveryNotificationCommand> logger,
            ICertificateService certificateService,
            IFileTransferClient fileTransferClient,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _certificateService = certificateService;
            _fileTransferClient = fileTransferClient;
            _sftpSettings = options?.Value;
        }

        public async Task Execute()
        {
            _logger.Log(LogLevel.Information, "Print Delivery Notification Function Started");

            var fileNames = await _fileTransferClient.GetFileNames(_sftpSettings.DeliveryNotificationDirectory, FilePattern);

            if (!fileNames.Any())
            {
                _logger.Log(LogLevel.Information, "No certificate delivery notifications from the printer are available to process");
                return;
            }

            await ProcessFiles(fileNames);
        }

        private async Task ProcessFiles(IEnumerable<string> fileNames)
        {
            var sortedFileNames = fileNames.ToList().SortByDateTimePattern(DateTimePattern, "ddMMyyHHmm");
            foreach (var fileName in sortedFileNames)
            {
                await ProcessFile(fileName);
            }
        }

        private async Task ProcessFile(string fileName)
        {
            var fileContent = _fileTransferClient.DownloadFile($"{_sftpSettings.DeliveryNotificationDirectory}/{fileName}");
            var receipt = JsonConvert.DeserializeObject<DeliveryReceipt>(fileContent);

            using (var validationService = new ValidationService(fileName, fileContent, _sftpSettings.ErrorDeliveryNotificationDirectory, _fileTransferClient))
            {
                if (receipt?.DeliveryNotifications == null)
                {
                    _logger.Log(LogLevel.Information, $"Could not process delivery receipt file due to invalid format [{fileName}]");
                    validationService.Log(nameof(receipt.DeliveryNotifications), $"Could not process delivery receipt file due to invalid format [{fileName}]");
                    return;
                }

                var invalidDeliveryNotificationStatuses = receipt.DeliveryNotifications
                    .GroupBy(certificateDeliveryNotificationStatus => certificateDeliveryNotificationStatus.Status)
                    .Select(certificateDeliveryNotificationStatus => certificateDeliveryNotificationStatus.Key)
                    .Where(deliveryNotificationStatus => !DeliveryNotificationStatus.Contains(deliveryNotificationStatus))
                    .ToList();

                foreach(var invalidDeliveryNotificationStatus in invalidDeliveryNotificationStatuses)
                {
                    _logger.Log(LogLevel.Information, $"The certificate status {invalidDeliveryNotificationStatus} is not a valid delivery notification status.");
                    validationService.Log("Status", $"The certificate status {invalidDeliveryNotificationStatus} is not a valid delivery notification status.");
                }

                var result = await _certificateService.Save(receipt.DeliveryNotifications.Select(n => new Certificate
                {
                    BatchId = n.BatchID,
                    CertificateReference = n.CertificateNumber,
                    Status = n.Status,
                    StatusDate = n.StatusChangeDate,
                    Reason = n.Reason
                }));

                _fileTransferClient.MoveFile($"{_sftpSettings.DeliveryNotificationDirectory}/{fileName}", _sftpSettings.ArchiveDeliveryNotificationDirectory);

                foreach (var error in result.Errors)
                {
                    validationService.Log(error.Field, error.ErrorMessage);
                }
            }
        }
    }
}
