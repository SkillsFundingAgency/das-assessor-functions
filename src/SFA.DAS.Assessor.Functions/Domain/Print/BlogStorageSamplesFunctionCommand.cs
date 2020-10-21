using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class BlogStorageSamplesFunctionCommand : IBlogStorageSamplesFunctionCommand
    {
        private readonly ILogger<PrintProcessCommand> _logger;
        private readonly IFileTransferClient _fileTransferClient;

        private string _printResponseDirectory;
        private string _printReponseBlobContainerName;

        private string _deliveryNotificationDirectory;
        private string _deliveryNotificationBlobContainerName;

        public BlogStorageSamplesFunctionCommand(
            ILogger<PrintProcessCommand> logger,
            IFileTransferClient fileTransferClient,
            IOptions<CertificatePrintNotificationFunctionSettings> optionsCertificatePrintNotificationFunction,
            IOptions<CertificateDeliveryNotificationFunctionSettings> optionsCertificateDeliveryNotificationFunction)
        {
            _logger = logger;
            
            _fileTransferClient = fileTransferClient;

            _printResponseDirectory = optionsCertificatePrintNotificationFunction.Value.PrintResponseDirectory;
            _printReponseBlobContainerName = optionsCertificatePrintNotificationFunction.Value.PrintResponseExternalBlobContainer;

            _deliveryNotificationDirectory = optionsCertificateDeliveryNotificationFunction.Value.DeliveryNotificationDirectory;
            _deliveryNotificationBlobContainerName = optionsCertificateDeliveryNotificationFunction.Value.DeliveryNotificationExternalBlobContainer;
        }

        public async Task Execute()
        {
            try
            {
                await UploadSamplePrintResponseFile();
                await UploadSampleDeliveryNotificationFile();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BlogStorageSamplesFunctionCommand failed");
                throw;
            }
        }

        private async Task UploadSamplePrintResponseFile()
        {
            var samplePrintResponse = new BatchData()
            {
                BatchNumber = 1,
                BatchDate = DateTime.Parse("2020-01-31T13:30:00.0000000Z"),
                PostalContactCount = 22,
                TotalCertificateCount = 48,
                ProcessedDate = DateTime.Parse("2020-02-03T15:30:00.0000000Z")
            };

            var filename = "PrintBatchResponse-001-3101201330.json";
            var path = $"{_printResponseDirectory}/Samples/{filename}";

            _fileTransferClient.ContainerName = _printReponseBlobContainerName;
            
            await UploadSampleFile(path, JsonConvert.SerializeObject(samplePrintResponse));
        }

        private async Task UploadSampleDeliveryNotificationFile()
        {
            var sampleDeliveryNotification = new DeliveryReceipt()
            {
                DeliveryNotifications = new List<DeliveryNotification>()
                {
                    new DeliveryNotification()
                    {
                        BatchID = 1,
                        CertificateNumber = "00000001",
                        Status = "Delivered",
                        Reason = "",
                        StatusChangeDate = DateTime.Parse("2020-04-03T16:31:40.0000000Z")
                    },
                    new DeliveryNotification()
                    {
                        BatchID = 1,
                        CertificateNumber = "00000001",
                        Status = "NotDelivered",
                        Reason = "Reason why certificate wasn't delivered",
                        StatusChangeDate = DateTime.Parse("2020-04-04T11:22:00.0000000Z")
                    }
                }
            };

            var filename = "DeliveryNotifications-0702201530.json";
            var path = $"{_deliveryNotificationDirectory}/Samples/{filename}";

            _fileTransferClient.ContainerName = _deliveryNotificationBlobContainerName;

            await UploadSampleFile(path, JsonConvert.SerializeObject(sampleDeliveryNotification));
        }

        private async Task UploadSampleFile(string path, string fileContents)
        {
            byte[] array = Encoding.ASCII.GetBytes(fileContents);
            using (var stream = new MemoryStream(array))
            {
                await _fileTransferClient.UploadFile(stream, path);
            }
        }
    }
}
