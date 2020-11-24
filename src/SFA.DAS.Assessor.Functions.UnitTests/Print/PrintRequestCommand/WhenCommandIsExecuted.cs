using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.PrintProcessCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Print.PrintRequestCommand _sut;

        private Mock<ILogger<Domain.Print.PrintRequestCommand>> _mockLogger;
        private Mock<IPrintCreator> _mockPrintCreator;
        private Mock<IBatchService> _mockBatchService;
        private Mock<ICertificateService> _mockCertificateService;
        private Mock<IScheduleService> _mockScheduleService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IExternalBlobFileTransferClient> _mockExternalFileTransferClient;
        private Mock<IInternalBlobFileTransferClient> _mockInternalFileTransferClient;
        private Mock<IOptions<PrintRequestOptions>> _mockOptions;

        private int _batchNumber = 1;
        private Guid _scheduleId = Guid.NewGuid();
        private List<Certificate> _certificates;
        private List<string> _uploadedFiles;
        private Batch _batch;
        private Guid _batchId;
        private PrintRequestOptions _options;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintRequestCommand>>();
            _mockPrintCreator = new Mock<IPrintCreator>();
            _mockBatchService = new Mock<IBatchService>();
            _mockCertificateService = new Mock<ICertificateService>();
            _mockScheduleService = new Mock<IScheduleService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockExternalFileTransferClient = new Mock<IExternalBlobFileTransferClient>();
            _mockInternalFileTransferClient = new Mock<IInternalBlobFileTransferClient>();
            _mockOptions = new Mock<IOptions<PrintRequestOptions>>();

            _options = new PrintRequestOptions {
                Directory = "MockPrintRequestDirectory",
                ArchiveDirectory = "MockArchivePrintRequestDirectory"
            };

            _mockOptions
                .Setup(m => m.Value)
                .Returns(_options);

            _uploadedFiles = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < 10; i++)
            {
                var filename = $"PrintBatch-12{i}-{generator.Next(DateTime.Now.AddDays(-100), DateTime.Now.AddDays(100)).ToString("ddMMyyHHmm")}.json";
                _uploadedFiles.Add(filename);

                _mockExternalFileTransferClient
                    .Setup(m => m.DownloadFile($"{_options.Directory}/{filename}"))
                    .ReturnsAsync(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchNumber = _batchNumber.ToString(), BatchDate = DateTime.Now } }));
            };

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), false))
                .ReturnsAsync(_uploadedFiles);

            _mockScheduleService
                .Setup(m => m.Get())
                .ReturnsAsync(new Schedule { Id = _scheduleId, RunTime = DateTime.Now });

            _mockBatchService
                .Setup(m => m.NextBatchId())
                .ReturnsAsync(_batchNumber);

            _mockPrintCreator
                .Setup(m => m.Create(It.IsAny<int>(), It.IsAny<List<Certificate>>()))
                .Returns("{}");

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

            _sut = new Domain.Print.PrintRequestCommand(
                _mockLogger.Object,
                _mockPrintCreator.Object,
                _mockBatchService.Object,
                _mockCertificateService.Object,
                _mockScheduleService.Object,
                _mockNotificationService.Object,
                _mockExternalFileTransferClient.Object,
                _mockInternalFileTransferClient.Object,
                _mockOptions.Object
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
            _mockNotificationService.Verify(m => m.SendPrintRequest(It.IsAny<int>(), It.IsAny<List<Certificate>>(), It.IsAny<string>()), Times.Never);
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
        public async Task ThenItShouldNotCreateAJsonPrintOutputIfNotScheduledToRun()
        {
            // Arrange
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockPrintCreator.Verify(m => m.Create(It.IsAny<int>(), It.IsAny<List<Certificate>>()), Times.Never);
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
            _mockNotificationService.Verify(m => m.SendPrintRequest(It.IsAny<int>(), It.IsAny<List<Certificate>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldCreateAJsonPrint()
        {
            // Act
            await _sut.Execute();

            // Assert
            _mockPrintCreator.Verify(m => m.Create(_batchNumber, _certificates), Times.Once);
            _mockNotificationService.Verify(m => m.SendPrintRequest(_batchNumber, _certificates, It.IsAny<string>()), Times.Once);
            _mockBatchService.Verify(m => m.Save(It.Is<Batch>(b => b.BatchNumber.Equals(_batchNumber))), Times.Once);
            _mockScheduleService.Verify(m => m.Save(It.Is<Schedule>(s => s.Id == _scheduleId)), Times.Once);
        }

        [Test]
        public async Task ThenItShouldUploadAndArchivePrintRequest()
        {
            // Arrange
            var fileName = $"PrintBatch-{_batchNumber.ToString().PadLeft(3, '0')}-{DateTime.UtcNow.UtcToTimeZoneTime():ddMMyyHHmm}.json";
            
            // Act
            await _sut.Execute();

            // Assert
            _mockExternalFileTransferClient.Verify(m => m.UploadFile(It.IsAny<string>(), It.Is<string>(s => s == $"{_options.Directory}/{fileName}")));
            _mockInternalFileTransferClient.Verify(m => m.UploadFile(It.IsAny<string>(), It.Is<string>(s => s == $"{_options.ArchiveDirectory}/{fileName}")));
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

        [Test]
        public async Task ThenItShouldUpdateTheLastRunStatusToStarted()
        {
            // Arrange

            // Act
            await _sut.Execute();

            // Assert
            _mockScheduleService.Verify(q => q.Start(It.IsAny<Schedule>()), Times.Once());
        }
    }
}
