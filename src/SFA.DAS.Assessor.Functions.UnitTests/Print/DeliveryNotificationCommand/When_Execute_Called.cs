﻿using FizzWare.NBuilder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.DeliveryNotificationCommand
{
    public class When_Execute_Called
    {
        private Domain.Print.DeliveryNotificationCommand _sut;

        private Mock<ILogger<Domain.Print.DeliveryNotificationCommand>> _mockLogger;
        private Mock<ICertificateService> _mockCertificateService;
        private Mock<IExternalBlobFileTransferClient> _mockExternalFileTransferClient;
        private Mock<IInternalBlobFileTransferClient> _mockInternalFileTransferClient;
        private Mock<IOptions<DeliveryNotificationOptions>> _mockOptions;

        private int _batchNumber = 1;
        private List<string> _downloadedFiles;
        private DeliveryNotificationOptions _options;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.DeliveryNotificationCommand>>();
            _mockCertificateService = new Mock<ICertificateService>();
            _mockExternalFileTransferClient = new Mock<IExternalBlobFileTransferClient>();
            _mockInternalFileTransferClient = new Mock<IInternalBlobFileTransferClient>();
            _mockOptions = new Mock<IOptions<DeliveryNotificationOptions>>();

            _options = new DeliveryNotificationOptions
            {
                Directory = "MockDeliveryNotificationDirectory",
                ArchiveDirectory = "MockArchiveDeliveryNotificationDirectory"
            };

            _mockOptions
                .Setup(m => m.Value)
                .Returns(_options);

            _downloadedFiles = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < 10; i++)
            {
                var filename = $"DeliveryNotifications-{generator.Next(DateTime.Now.AddDays(-100), DateTime.Now.AddDays(100)).ToString("ddMMyyHHmm")}.json";
                _downloadedFiles.Add(filename);

                _mockExternalFileTransferClient
                    .Setup(m => m.DownloadFile($"{_options.Directory}/{filename}"))
                    .ReturnsAsync(JsonConvert.SerializeObject(new DeliveryReceipt { DeliveryNotifications = new List<DeliveryNotification> 
                        { new DeliveryNotification { BatchID = _batchNumber } } }));
            };

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(_downloadedFiles);

            _sut = new Domain.Print.DeliveryNotificationCommand(
                _mockLogger.Object,
                _mockCertificateService.Object,
                _mockExternalFileTransferClient.Object,
                _mockInternalFileTransferClient.Object,
                _mockOptions.Object
                );
        }

        [Test]
        public async Task ThenItShouldLogTheStartOfTheProcess()
        {
            // Arrange
            var logMessage = "DeliveryNotificationCommand - Started";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoDeliveryNotificationsToProcess()
        {
            // Arrange
            var logMessage = "DeliveryNotificationCommand - No certificate delivery notifications from the printer are available to process";
            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
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
            var exceptionLogMessage = $"The delivery notification file [{fileName}] contained invalid entries, an error file has been created";
            var logMessage = $"DeliveryNotificationCommand - Could not process delivery notification file [{fileName}]";

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(new List<string> { fileName });

            _mockExternalFileTransferClient
                .Setup(m => m.DownloadFile($"{_options.Directory}/{fileName}"))
                .ReturnsAsync("{}");

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)),
                It.Is<Exception>(p => p.Message.StartsWith(exceptionLogMessage)),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldProcessAndMoveToArchiveDeliveryNotificationFiles()
        {
            // Act
            await _sut.Execute();

            // Assert
            foreach (var filename in _downloadedFiles)
            {
                var downloadedFilename = $"{_options.Directory}/{filename}";
                var uploadedFilename = $"{_options.ArchiveDirectory}/{filename}";

                _mockExternalFileTransferClient.Verify(m => m.DownloadFile(downloadedFilename), Times.Once);
                _mockInternalFileTransferClient.Verify(m => m.UploadFile(It.IsAny<string>(), uploadedFilename), Times.Once);
                _mockExternalFileTransferClient.Verify(m => m.DeleteFile(downloadedFilename), Times.Once);
            }
        }
    }
}
