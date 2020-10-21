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

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.PrintNotificationCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Print.PrintNotificationCommand _sut;

        private Mock<ILogger<Domain.Print.PrintNotificationCommand>> _mockLogger;
        private Mock<IBatchService> _mockBatchService;
        private Mock<IFileTransferClient> _mockExternalFileTransferClient;
        private Mock<IFileTransferClient> _mockInternalFileTransferClient;
        private Mock<IOptions<CertificatePrintNotificationFunctionSettings>> _mockSettings;

        private int _batchNumber = 1;
        private List<string> _downloadedFiles;
        private CertificatePrintNotificationFunctionSettings _settings;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintNotificationCommand>>();
            _mockBatchService = new Mock<IBatchService>();
            _mockExternalFileTransferClient = new Mock<IFileTransferClient>();
            _mockInternalFileTransferClient = new Mock<IFileTransferClient>();
            _mockSettings = new Mock<IOptions<CertificatePrintNotificationFunctionSettings>>();

            _settings = new CertificatePrintNotificationFunctionSettings {
                PrintResponseDirectory = "MockPrintResponseDirectory",
                ArchivePrintResponseDirectory = "MockArchivePrintResponseDirectory"
            };

            _mockSettings
                .Setup(m => m.Value)
                .Returns(_settings);

            _downloadedFiles = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < 10; i++)
            {
                var filename = $"PrintResponse-0120-{generator.Next(111111, 999999)}.json";
                _downloadedFiles.Add(filename);

                _mockExternalFileTransferClient
                    .Setup(m => m.DownloadFile($"{_settings.PrintResponseDirectory}/{filename}"))
                    .ReturnsAsync(JsonConvert.SerializeObject(new PrintReceipt { 
                        Batch = new BatchData { 
                            BatchNumber = _batchNumber,
                            BatchDate = DateTime.Now.AddDays(-1),
                            ProcessedDate = DateTime.Now,
                            PostalContactCount = 2,
                            TotalCertificateCount = 6
                        }}));
            };

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(_downloadedFiles);

            _mockBatchService
                .Setup(m => m.Get(_batchNumber))
                .ReturnsAsync(new Batch { BatchNumber = _batchNumber });

            _sut = new Domain.Print.PrintNotificationCommand(
                _mockLogger.Object,
                _mockBatchService.Object,
                _mockExternalFileTransferClient.Object,
                _mockInternalFileTransferClient.Object,
                _mockSettings.Object
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
            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
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

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(new List<string> { fileName });

            _mockExternalFileTransferClient
                .Setup(m => m.DownloadFile($"{_settings.PrintResponseDirectory}/{fileName}"))
                .ReturnsAsync("{}");

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

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(new List<string> { fileName });

            _mockExternalFileTransferClient
                .Setup(m => m.DownloadFile($"{_settings.PrintResponseDirectory}/{fileName}"))
               .ReturnsAsync(JsonConvert.SerializeObject(new PrintReceipt
               {
                   Batch = new BatchData
                   {
                       BatchNumber = _batchNumber,
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
        public async Task ThenItShouldProcessAndArchivePrintNotificationFiles()
        {
            // Act
            await _sut.Execute();

            // Assert
            _mockBatchService.Verify(m => m.Save(It.Is<Batch>(b => b.BatchNumber == _batchNumber)), Times.Exactly(_downloadedFiles.Count));
            
            foreach (var filename in _downloadedFiles)
            {
                var downloadedFilename = $"{_settings.PrintResponseDirectory}/{filename}";
                var uploadedFilename = $"{_settings.ArchivePrintResponseDirectory}/{filename}";

                _mockExternalFileTransferClient.Verify(m => m.MoveFile(downloadedFilename, _mockInternalFileTransferClient.Object, uploadedFilename), Times.Once);
            }
        }
    }
}
