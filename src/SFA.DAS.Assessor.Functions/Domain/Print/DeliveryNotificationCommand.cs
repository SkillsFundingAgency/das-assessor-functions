using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class DeliveryNotificationCommand : NotificationCommand, IDeliveryNotificationCommand
    {
        // DeliveryNotifications-ddMMyyHHmm.json
        private static readonly string DateTimePattern = "[0-9]{10}";
        private static readonly string FilePattern = $@"^[Dd][Ee][Ll][Ii][Vv][Ee][Rr][Yy][Nn][Oo][Tt][Ii][Ff][Ii][Cc][Aa][Tt][Ii][Oo][Nn][Ss]-{DateTimePattern}.json";

        private readonly ILogger<DeliveryNotificationCommand> _logger;
        private readonly ICertificateService _certificateService;
        private readonly DeliveryNotificationOptions _options;

        private ICollector<string> _messageQueue;

        public DeliveryNotificationCommand(
            ILogger<DeliveryNotificationCommand> logger,
            ICertificateService certificateService,
            IExternalBlobFileTransferClient externalFileTransferClient,
            IInternalBlobFileTransferClient internalFileTransferClient,
            IOptions<DeliveryNotificationOptions> options)
            : base (externalFileTransferClient, internalFileTransferClient)
        {
            _logger = logger;
            _certificateService = certificateService;
            _options = options?.Value;
        }

        public async Task Execute(ICollector<string> messageQueue)
        {
            _logger.Log(LogLevel.Information, "Print Delivery Notification Function Started");

            _messageQueue = messageQueue;

            var fileNames = await _externalFileTransferClient.GetFileNames(_options.Directory, FilePattern, false);

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
            var fileContents = await _externalFileTransferClient.DownloadFile($"{_options.Directory}/{fileName}");
            var receipt = JsonConvert.DeserializeObject<DeliveryReceipt>(fileContents);

            if (receipt?.DeliveryNotifications == null)
            {
                _logger.LogInformation($"Could not process delivery notification file '{fileName}' due to invalid format");
                return;
            }

            var invalidDeliveryNotificationStatuses = receipt.DeliveryNotifications
                   .GroupBy(certificateDeliveryNotificationStatus => certificateDeliveryNotificationStatus.Status)
                   .Select(certificateDeliveryNotificationStatus => certificateDeliveryNotificationStatus.Key)
                   .Where(deliveryNotificationStatus => !CertificateStatus.HasDeliveryNotificationStatus(deliveryNotificationStatus))
                   .ToList();
                        
            invalidDeliveryNotificationStatuses.ForEach(invalidDeliveryNotificationStatus =>
            {
                _logger.LogError($"The delivery notification file '{fileName}' contained invalid delivery status '{invalidDeliveryNotificationStatus}'");
            });

            var validDeliveryNotifications = receipt.DeliveryNotifications
                .Where(deliveryNotification => CertificateStatus.HasDeliveryNotificationStatus(deliveryNotification.Status))
                .ToList();

            _certificateService.QueueCertificatePrintStatusUpdateMessages(validDeliveryNotifications.Select(n => new CertificatePrintStatusUpdate
            {
                CertificateReference = n.CertificateNumber,
                BatchNumber = n.BatchID,
                Status = n.Status,
                StatusAt = n.StatusChangeDate,
                ReasonForChange = n.Reason
            }).ToList(), _messageQueue, _options.PrintStatusUpdateChunkSize);

            await ArchiveFile(fileContents, fileName, _options.Directory, _options.ArchiveDirectory);
        }
    }
}
