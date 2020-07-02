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
