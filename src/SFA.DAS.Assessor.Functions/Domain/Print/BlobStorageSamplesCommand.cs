using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class BlobStorageSamplesCommand : IBlobStorageSamplesCommand
    {
        private readonly ILogger<BlobStorageSamplesCommand> _logger;
        private readonly IExternalBlobFileTransferClient _blobFileTransferClient;

        private readonly string _printResponseDirectory;
        private readonly string _deliveryNotificationDirectory;
        
        public BlobStorageSamplesCommand(
            ILogger<BlobStorageSamplesCommand> logger,
            
            IExternalBlobFileTransferClient blobFileTransferClient,
            IOptions<PrintResponseOptions> optionsPrintResponse,
            IOptions<DeliveryNotificationOptions> optionsDeliveryNotification)
        {
            _logger = logger;
            _blobFileTransferClient = blobFileTransferClient;
            _printResponseDirectory = optionsPrintResponse?.Value.Directory;
            _deliveryNotificationDirectory = optionsDeliveryNotification?.Value.Directory;
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
                _logger.LogError(ex, $"BlobStorageSamplesFunctionCommand failed");
                throw;
            }
        }

        private async Task UploadSamplePrintResponseFile()
        {
            var samplePrintResponse = new SampleBatchData()
            {
                BatchNumber = 1,
                BatchDate = "2020-01-31T13:30:00.0000000Z",
                PostalContactCount = 22,
                TotalCertificateCount = 48,
                ProcessedDate = "2020-02-03T15:30:00.0000000Z"
            };

            var filename = "PrintBatchResponse-001-3101201330.json";
            var path = $"{_printResponseDirectory}/Samples/{filename}";

            await _blobFileTransferClient.UploadFile(JsonConvert.SerializeObject(samplePrintResponse), path);
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
                        Status = CertificateStatus.Delivered,
                        Reason = "",
                        StatusChangeDate = DateTime.Parse("2020-04-03T16:31:40.0000000Z")
                    },
                    new DeliveryNotification()
                    {
                        BatchID = 1,
                        CertificateNumber = "00000001",
                        Status = CertificateStatus.NotDelivered,
                        Reason = "Reason why certificate wasn't delivered",
                        StatusChangeDate = DateTime.Parse("2020-04-04T11:22:00.0000000Z")
                    }
                }
            };

            var filename = "DeliveryNotifications-0702201530.json";
            var path = $"{_deliveryNotificationDirectory}/Samples/{filename}";

            await _blobFileTransferClient.UploadFile(JsonConvert.SerializeObject(sampleDeliveryNotification), path);
        }
    }
}
