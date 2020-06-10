using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Linq;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System.Collections.Generic;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class DeliveryNotificationCommand : IDeliveryNotificationCommand
    {
        // DeliveryNotification-ddMMyyHHmm.json
        const string FilePattern = @"^[Dd][Ee][Ll][Ii][Vv][Ee][Rr][Yy][Nn][Oo][Tt][Ii][Ff][Ii][Cc][Aa][Tt][Ii][Oo][Nn]-[0-9]{10}.json";

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
            foreach (var fileName in fileNames)
            {
                await ProcessFile(fileName);
            }
        }

        private async Task ProcessFile(string fileName)
        {
            var receipt = JsonConvert.DeserializeObject<DeliveryReceipt>(_fileTransferClient.DownloadFile($"{_sftpSettings.DeliveryNotificationDirectory}/{fileName}"));

            if (receipt?.DeliveryNotifications == null)
            {
                _logger.Log(LogLevel.Information, $"Could not process delivery receipt file due to invalid format [{fileName}]");
                return;
            }

            await _certificateService.Save(receipt.DeliveryNotifications.Select(n => new Certificate
            {
                BatchId = n.BatchID,
                CertificateReference = n.CertificateNumber,
                Status = n.Status,
                StatusDate = n.StatusChangeDate
            }));

            _fileTransferClient.DeleteFile($"{_sftpSettings.DeliveryNotificationDirectory}/{fileName}");
        }
    }
}
