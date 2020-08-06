using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.PrintProcessCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Print.PrintProcessCommand _sut;

        private Mock<ILogger<Domain.Print.PrintProcessCommand>> _mockLogger;
        private Mock<IPrintingJsonCreator> _mockPrintingJsonCreator;
        private Mock<IPrintingSpreadsheetCreator> _mockPrintingSpreadsheetCreator;
        private Mock<IBatchService> _mockBatchService;
        private Mock<ICertificateService> _mockCertificateService;
        private Mock<IScheduleService> _mockScheduleService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IFileTransferClient> _mockFileTransferClient;
        private Mock<IOptions<SftpSettings>> _mockSftpSettings;

        private int _batchNumber = 1;
        private Guid _scheduleId = Guid.NewGuid();
        private List<Certificate> _certificates;
        private List<string> _downloadedFiles;
        private Batch _batch;
        private Guid _batchId;
        private SftpSettings _sftpSettings;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintProcessCommand>>();
            _mockPrintingJsonCreator = new Mock<IPrintingJsonCreator>();
            _mockPrintingSpreadsheetCreator = new Mock<IPrintingSpreadsheetCreator>();
            _mockBatchService = new Mock<IBatchService>();
            _mockCertificateService = new Mock<ICertificateService>();
            _mockScheduleService = new Mock<IScheduleService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockFileTransferClient = new Mock<IFileTransferClient>();
            _mockSftpSettings = new Mock<IOptions<SftpSettings>>();

            _sftpSettings = new SftpSettings { UseJson = true, ProofDirectory = "Test" };

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
                    .Setup(m => m.DownloadFile($"{_sftpSettings.ProofDirectory}/{filename}"))
                    .Returns(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchNumber = _batchNumber.ToString(), BatchDate = DateTime.Now } }));
            };

            _mockFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>()))
                .ReturnsAsync(_downloadedFiles);

            _mockScheduleService
                .Setup(m => m.Get())
                .ReturnsAsync(new Schedule { Id = _scheduleId, RunTime = DateTime.Now });

            _mockBatchService
                .Setup(m => m.NextBatchId())
                .ReturnsAsync(_batchNumber);

            _certificates = Builder<Certificate>
                .CreateListOfSize(10)
                .All()
                .Build() as List<Certificate>;

            _mockCertificateService
                .Setup(m => m.Get(Domain.Print.Interfaces.CertificateStatus.ToBePrinted))
                .ReturnsAsync(_certificates);

            _batchId = Guid.NewGuid();
            _batch = new Batch { Id = _batchId, BatchNumber = _batchNumber };

            _mockBatchService
                .Setup(m => m.Get(_batchNumber))
                .ReturnsAsync(_batch);

            _sut = new Domain.Print.PrintProcessCommand(
                _mockLogger.Object,
                _mockPrintingJsonCreator.Object,
                _mockPrintingSpreadsheetCreator.Object,
                _mockBatchService.Object,
                _mockCertificateService.Object,
                _mockScheduleService.Object,
                _mockNotificationService.Object,
                _mockFileTransferClient.Object,
                _mockSftpSettings.Object
                );
        }

        [Test]
        public async Task ThenItShouldLogTheStartOfTheProcess()
        {
            // Arrange
            var logMessage = "Print Process Function Started";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfNotScheduledToRun()
        {
            // Arrange
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            var logMessage = "Print Function not scheduled to run at this time.";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldNotCallTheNotificationServiceIfNotScheduledToRun()
        {
            // Arrange
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockNotificationService.Verify(m => m.Send(It.IsAny<int>(), It.IsAny<List<Certificate>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldNotChangeAnyDataIfNotScheduledToRun()
        {
            // Arrange
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            // Act
            await _sut.Execute();

            // Assert            
            _mockBatchService.Verify(m => m.Save(It.IsAny<Batch>()), Times.Never);
            _mockScheduleService.Verify(q => q.Save(It.IsAny<Schedule>()), Times.Never());
        }

        [Test]
        public async Task ThenItShouldNotCreateASpreadSheetIfNotScheduledToRun()
        {
            // Arrange
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            _sftpSettings.UseJson = false;
            
            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingSpreadsheetCreator.Verify(m => m.Create(It.IsAny<int>(), It.IsAny<IEnumerable<Certificate>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldNotCreateAJsonPrintOutputIfNotScheduledToRun()
        {
            // Arrange
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            _sftpSettings.UseJson = true;

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingJsonCreator.Verify(m => m.Create(It.IsAny<int>(), It.IsAny<List<Certificate>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoCertificatesToProcess()
        {
            // Arrange
            var logMessage = "No certificates to process";

            _mockCertificateService
                .Setup(m => m.Get(Domain.Print.Interfaces.CertificateStatus.ToBePrinted))
                .ReturnsAsync(new List<Certificate>());

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldOnlyCompleteScheduleIfThereAreNoCertificatesToProcess()
        {
            // Arrange
            _mockCertificateService
               .Setup(m => m.Get(Domain.Print.Interfaces.CertificateStatus.ToBePrinted))
               .ReturnsAsync(new List<Certificate>());

            // Act
            await _sut.Execute();

            // Assert            
            _mockScheduleService.Verify(q => q.Save(It.Is<Schedule>(s => s.Id ==_scheduleId)), Times.Once());

            _mockBatchService.Verify(m => m.Save(It.IsAny<Batch>()), Times.Never);
            _mockNotificationService.Verify(m => m.Send(It.IsAny<int>(), It.IsAny<List<Certificate>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldCreateASpreadSheetIfConfiguredTo()
        {
            // Arrange
            _sftpSettings.UseJson = false;

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingSpreadsheetCreator.Verify(m => m.Create(_batchNumber, _certificates, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldCreateAJsonPrintIfConfiguredTo()
        {
            // Arrange
            _sftpSettings.UseJson = true;

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingJsonCreator.Verify(m => m.Create(_batchNumber, _certificates, It.IsAny<string>()), Times.Once);
            _mockNotificationService.Verify(m => m.Send(_batchNumber, _certificates, It.IsAny<string>()), Times.Once);
            _mockBatchService.Verify(m => m.Save(It.Is<Batch>(b => b.BatchNumber.Equals(_batchNumber))), Times.Once);
            _mockScheduleService.Verify(m => m.Save(It.Is<Schedule>(s => s.Id == _scheduleId)), Times.Once);
        }

        [Test]
        public async Task ThenItShouldCreateASpreadSheetPrintIfConfiguredTo()
        {
            // Arrange
            _sftpSettings.UseJson = false;

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingSpreadsheetCreator.Verify(m => m.Create(_batchNumber, _certificates, It.IsAny<string>()), Times.Once);
            _mockNotificationService.Verify(m => m.Send(_batchNumber, _certificates, It.IsAny<string>()), Times.Once);
            _mockBatchService.Verify(m => m.Save(It.Is<Batch>(b => b.BatchNumber.Equals(_batchNumber))), Times.Once);
            _mockScheduleService.Verify(m => m.Save(It.Is<Schedule>(s => s.Id == _scheduleId)), Times.Once);
        }

        [Test]
        public async Task ThenItShouldGetTheNextBatchNumber()
        {
            // Arrange

            // Act
            await _sut.Execute();

            // Assert
            _mockBatchService.Verify(q => q.NextBatchId(), Times.Once());
        }
    }
}
