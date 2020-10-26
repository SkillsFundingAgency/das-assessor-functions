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
        private readonly IFileTransferClient _externalFileTransferClient;
        private readonly IFileTransferClient _internalFileTransferClient;
        private readonly CertificateDeliveryNotificationFunctionSettings _settings;

        public DeliveryNotificationCommand(
            ILogger<DeliveryNotificationCommand> logger,
            ICertificateService certificateService,
            IFileTransferClient externalFileTransferClient,
            IFileTransferClient internalFileTransferClient,
            IOptions<CertificateDeliveryNotificationFunctionSettings> options)
        {
            _logger = logger;
            _certificateService = certificateService;
            _externalFileTransferClient = externalFileTransferClient;
            _internalFileTransferClient = internalFileTransferClient;
            _settings = options?.Value;

            _externalFileTransferClient.ContainerName = _settings.DeliveryNotificationExternalBlobContainer;
            _internalFileTransferClient.ContainerName = _settings.DeliveryNotificationInternalBlobContainer;
        }

        public async Task Execute()
        {
            _logger.Log(LogLevel.Information, "Print Delivery Notification Function Started");

            var fileNames = await _externalFileTransferClient.GetFileNames(_settings.DeliveryNotificationDirectory, FilePattern, false);

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
            var fileContents = await _externalFileTransferClient.DownloadFile($"{_settings.DeliveryNotificationDirectory}/{fileName}");
            var receipt = JsonConvert.DeserializeObject<DeliveryReceipt>(fileContents);

            if (receipt?.DeliveryNotifications == null)
            {
                _logger.Log(LogLevel.Information, $"Could not process delivery receipt file due to invalid format [{fileName}]");
                return;
            }

            var invalidDeliveryNotificationStatuses = receipt.DeliveryNotifications
                   .GroupBy(certificateDeliveryNotificationStatus => certificateDeliveryNotificationStatus.Status)
                   .Select(certificateDeliveryNotificationStatus => certificateDeliveryNotificationStatus.Key)
                   .Where(deliveryNotificationStatus => !DeliveryNotificationStatus.Contains(deliveryNotificationStatus))
                   .ToList();
                        
            invalidDeliveryNotificationStatuses.ForEach(invalidDeliveryNotificationStatus =>
            {
                _logger.Log(LogLevel.Information, $"The certificate status {invalidDeliveryNotificationStatus} is not a valid delivery notification status.");
                
            });

            await _certificateService.Save(receipt.DeliveryNotifications.Select(n => new Certificate
            {
                BatchId = n.BatchID,
                CertificateReference = n.CertificateNumber,
                Status = n.Status,
                StatusDate = n.StatusChangeDate,
                Reason = n.Reason
            }));

            await _internalFileTransferClient.UploadFile(fileContents, $"{_settings.ArchiveDeliveryNotificationDirectory}/{fileName}");
            await _externalFileTransferClient.DeleteFile($"{_settings.DeliveryNotificationDirectory}/{fileName}");
        }
    }
}
