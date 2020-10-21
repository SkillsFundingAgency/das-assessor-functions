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
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using BatchData = SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications.BatchData;

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.PrintNotificationCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Print.PrintNotificationCommand _sut;

        private Mock<ILogger<Domain.Print.PrintNotificationCommand>> _mockLogger;
        private Mock<IBatchService> _mockBatchService;
        private Mock<IFileTransferClient> _mockFileTransferClient;
        private Mock<IOptions<SftpSettings>> _mockSftpSettings;

        private int _batchNumber = 1;
        private List<string> _downloadedFiles;
        private SftpSettings _sftpSettings;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintNotificationCommand>>();
            _mockBatchService = new Mock<IBatchService>();
            _mockFileTransferClient = new Mock<IFileTransferClient>();
            _mockSftpSettings = new Mock<IOptions<SftpSettings>>();

            _sftpSettings = new SftpSettings { UseJson = true, PrintResponseDirectory = "TestNotification",
                ArchivePrintResponseDirectory = "ArchivePrintResponseTestPrint" };

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
                    .Setup(m => m.DownloadFile($"{_sftpSettings.PrintResponseDirectory}/{filename}"))
                    .Returns(JsonConvert.SerializeObject(new PrintReceipt { 
                        Batch = new BatchData { 
                            BatchNumber = _batchNumber.ToString(),
                            BatchDate = DateTime.Now.AddDays(-1),
                            ProcessedDate = DateTime.Now,
                            PostalContactCount = 2,
                            TotalCertificateCount = 6
                        }}));
            };

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_downloadedFiles);

            _mockBatchService
                .Setup(m => m.Get(_batchNumber))
                .ReturnsAsync(new Batch { BatchNumber = _batchNumber });

            _mockBatchService
                .Setup(m => m.Save(It.IsAny<Batch>()))
                .ReturnsAsync(new ValidationResponse());

            _sut = new Domain.Print.PrintNotificationCommand(
                _mockLogger.Object,
                _mockBatchService.Object,
                _mockFileTransferClient.Object,
                _mockSftpSettings.Object
                );
        }

        [Test]
        public async Task ThenItShouldLogTheStartOfTheProcess()
        {
            // Arrange
            var logMessage = "Print Response Notification Function Started";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoPrintNotificationsToProcess()
        {
            // Arrange
            var logMessage = "There are no certificate print notifications from the printer to process";
            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string>());

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThePrintNotificationHasAnInvalidFormat()
        {
            // Arrange
            var fileName = Guid.NewGuid().ToString();
            var logMessage = $"Could not process print notifications due to invalid file format [{fileName}]";

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string> { fileName });

            _mockFileTransferClient
                .Setup(m => m.DownloadFile($"{_sftpSettings.PrintResponseDirectory}/{fileName}"))
                .Returns("{}");

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThePrintNotificationHasAnInvalidBatchNumber()
        {
            // Arrange
            var fileName = Guid.NewGuid().ToString();
            var logMessage = $"Could not process print notifications unable to match an existing batch Log Batch Number [{_batchNumber}] in the print notification in the file [{fileName}]";

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string> { fileName });

            _mockFileTransferClient
                .Setup(m => m.DownloadFile($"{_sftpSettings.PrintResponseDirectory}/{fileName}"))
               .Returns(JsonConvert.SerializeObject(new PrintReceipt
               {
                   Batch = new BatchData
                   {
                       BatchNumber = _batchNumber.ToString(),
                       BatchDate = DateTime.Now.AddDays(-1),
                       ProcessedDate = DateTime.Now,
                       PostalContactCount = 2,
                       TotalCertificateCount = 6
                   }
               }));

            _mockBatchService
               .Setup(m => m.Get(_batchNumber))
               .Returns(Task.FromResult<Batch>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThePrintNotificationHasAnNonIntegerBatchNumber()
        {
            // Arrange
            var fileName = Guid.NewGuid().ToString();
            var batchNumber = "NotAnInt";
            var logMessage = $"Could not process print notifications the Batch Number is not an integer [{batchNumber}] in the print notification in the file [{fileName}]";

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string> { fileName });

            _mockFileTransferClient
                .Setup(m => m.DownloadFile($"{_sftpSettings.PrintResponseDirectory}/{fileName}"))
               .Returns(JsonConvert.SerializeObject(new PrintReceipt
               {
                   Batch = new BatchData
                   {
                       BatchNumber = batchNumber,
                       BatchDate = DateTime.Now.AddDays(-1),
                       ProcessedDate = DateTime.Now,
                       PostalContactCount = 2,
                       TotalCertificateCount = 6
                   }
               }));

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldProcessAndArchivePrintNotificationFiles()
        {
            // Arrange

            // Act
            await _sut.Execute();

            // Assert
            _mockBatchService.Verify(m => m.Save(It.Is<Batch>(b => b.BatchNumber == _batchNumber)), Times.Exactly(_downloadedFiles.Count));
            _mockFileTransferClient.Verify(m => m.MoveFile(It.IsAny<string>(), _sftpSettings.ArchivePrintResponseDirectory), Times.Exactly(_downloadedFiles.Count));
        }
    }
}
