using FizzWare.NBuilder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintResponseCommand
{
    public class When_Execute_Called
    {
        private Domain.Print.PrintResponseCommand _sut;

        private Mock<ILogger<Domain.Print.PrintResponseCommand>> _mockLogger;
        private Mock<IBatchService> _mockBatchService;
        private Mock<IExternalBlobFileTransferClient> _mockExternalFileTransferClient;
        private Mock<IInternalBlobFileTransferClient> _mockInternalFileTransferClient;
        private Mock<IOptions<PrintResponseOptions>> _mockOptions;

        private int _batchNumber = 1;
        private List<string> _downloadedFiles;
        private PrintResponseOptions _options;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintResponseCommand>>();
            _mockBatchService = new Mock<IBatchService>();
            _mockExternalFileTransferClient = new Mock<IExternalBlobFileTransferClient>();
            _mockInternalFileTransferClient = new Mock<IInternalBlobFileTransferClient>();
            _mockOptions = new Mock<IOptions<PrintResponseOptions>>();

            _options = new PrintResponseOptions {
                Directory = "MockPrintResponseDirectory",
                ArchiveDirectory = "MockArchivePrintResponseDirectory"
            };

            _mockOptions
                .Setup(m => m.Value)
                .Returns(_options);

            _downloadedFiles = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < 10; i++)
            {
                var filename = $"PrintResponse-0120-{generator.Next(111111, 999999)}.json";
                _downloadedFiles.Add(filename);

                _mockExternalFileTransferClient
                    .Setup(m => m.DownloadFile($"{_options.Directory}/{filename}"))
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

            _mockBatchService
                .Setup(m => m.Update(It.IsAny<Batch>()))
                .ReturnsAsync(new List<CertificatePrintStatusUpdateMessage>());

            _sut = new Domain.Print.PrintResponseCommand(
                _mockLogger.Object,
                _mockBatchService.Object,
                _mockExternalFileTransferClient.Object,
                _mockInternalFileTransferClient.Object,
                _mockOptions.Object
                );
        }

        [Test]
        public async Task ThenItShouldLogTheStartOfTheProcess()
        {
            // Arrange
            var logMessage = "PrintResponseCommand - Started";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoPrintNotificationsToProcess()
        {
            // Arrange
            var logMessage = "PrintResponseCommand - There are no certificate print responses from the printer to process";
            
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
            var innerExceptionMessage = $"Could not process print response file [{fileName}] due to invalid file format";
            var exceptionMessage = $"The print response file [{fileName}] contained invalid entries, an error file has been created";
            var logMessage = $"PrintResponseCommand - Could not process print response file [{fileName}]";

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
                It.Is<Exception>(p => p.Message.StartsWith(exceptionMessage) && p.InnerException.Message.StartsWith(innerExceptionMessage)),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfThePrintNotificationHasAnInvalidBatchNumber()
        {
            // Arrange
            var fileName = Guid.NewGuid().ToString();

            var innerExceptionMessage = $"Could not process print response file [{fileName}] due to non matching Batch Number [{_batchNumber}]";
            var exceptionMessage = $"The print response file [{fileName}] contained invalid entries, an error file has been created";
            var logMessage = $"PrintResponseCommand - Could not process print response file [{fileName}]";

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(new List<string> { fileName });

            _mockExternalFileTransferClient
                .Setup(m => m.DownloadFile($"{_options.Directory}/{fileName}"))
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
            _mockLogger.Verify(m => m.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)),
                It.Is<Exception>(p => p.Message.StartsWith(exceptionMessage) && p.InnerException.Message.StartsWith(innerExceptionMessage)),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Test]
        public async Task ThenItShouldProcessAndArchivePrintNotificationFiles()
        {
            // Act
            await _sut.Execute();

            // Assert
            _mockBatchService.Verify(m => m.Update(
                It.Is<Batch>(b => b.BatchNumber == _batchNumber)), Times.Exactly(_downloadedFiles.Count));
            
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
