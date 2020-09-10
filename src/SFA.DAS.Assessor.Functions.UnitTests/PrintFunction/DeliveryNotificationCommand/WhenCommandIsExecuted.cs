using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.DeliveryNotificationCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Print.DeliveryNotificationCommand _sut;

        private Mock<ILogger<Domain.Print.DeliveryNotificationCommand>> _mockLogger;
        private Mock<ICertificateService> _mockCertificateService;
        private Mock<IFileTransferClient> _mockFileTransferClient;
        private Mock<IOptions<SftpSettings>> _mockSftpSettings;

        private int _batchNumber = 1;
        private List<string> _downloadedFiles;
        private SftpSettings _sftpSettings;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.DeliveryNotificationCommand>>();
            _mockCertificateService = new Mock<ICertificateService>();
            _mockFileTransferClient = new Mock<IFileTransferClient>();
            _mockSftpSettings = new Mock<IOptions<SftpSettings>>();

            _sftpSettings = new SftpSettings { UseJson = true, DeliveryNotificationDirectory = "TestDelivery" };

            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(_sftpSettings);

            _downloadedFiles = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < 10; i++)
            {
                var filename = $"printResponse-0120-{generator.Next(111111, 999999)}.json";
                _downloadedFiles.Add(filename);

                _mockFileTransferClient
                    .Setup(m => m.DownloadFile($"{_sftpSettings.DeliveryNotificationDirectory}/{filename}"))
                    .Returns(JsonConvert.SerializeObject(new DeliveryReceipt { DeliveryNotifications = new List<DeliveryNotification> { new DeliveryNotification { BatchID = _batchNumber } } }));
            };

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_downloadedFiles);

            _sut = new Domain.Print.DeliveryNotificationCommand(
                _mockLogger.Object,
                _mockCertificateService.Object,
                _mockFileTransferClient.Object,
                _mockSftpSettings.Object
                );
        }

        [Test]
        public async Task ThenItShouldLogTheStartOfTheProcess()
        {
            // Arrange
            var logMessage = "Print Delivery Notification Function Started";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoDeliveryNotificationsToProcess()
        {
            // Arrange
            var logMessage = "No certificate delivery notifications from the printer are available to process";
            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string>());

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfTheDeliveryNotificationHasAnInvalidFormat()
        {
            // Arrange
            var fileName = Guid.NewGuid().ToString();
            var logMessage = $"Could not process delivery receipt file due to invalid format [{fileName}]";

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string> { fileName });

            _mockFileTransferClient
                .Setup(m => m.DownloadFile($"{_sftpSettings.DeliveryNotificationDirectory}/{fileName}"))
                .Returns("{}");

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldProcessAndMoveToArchiveDeliveryNotificationFiles()
        {
            // Arrange

            // Act
            await _sut.Execute();

            // Assert
            _mockCertificateService.Verify(m => m.Save(It.Is<IEnumerable<Certificate>>(c => c.ToList().Where(i => i.BatchId == _batchNumber).Count().Equals(1))), Times.Exactly(_downloadedFiles.Count));
            _mockFileTransferClient.Verify(m => m.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(_downloadedFiles.Count));
        }
    }
}
