using FizzWare.NBuilder;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintRequestCommand
{
    public class When_Execute_Called
    {
        private Domain.Print.PrintRequestCommand _sut;

        private Mock<ILogger<Domain.Print.PrintRequestCommand>> _mockLogger;
        private Mock<IPrintCreator> _mockPrintCreator;
        private Mock<IBatchService> _mockBatchService;
        private Mock<IScheduleService> _mockScheduleService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IExternalBlobFileTransferClient> _mockExternalFileTransferClient;
        private Mock<IInternalBlobFileTransferClient> _mockInternalFileTransferClient;
        private Mock<IOptions<PrintRequestOptions>> _mockOptions;

        private readonly int _batchNumberWithCertificates = 1;
        private Guid _batchWithCertificatesId;
        private Batch _batchWithCertificates;

        private readonly int _batchNumberWithoutCertificates = 2;
        private Guid _batchWithoutCertificatesId;
        private Batch _batchWithoutCertificates;

        private List<CertificatePrintSummaryBase> _certificates;

        private Guid _scheduleId = Guid.NewGuid();
        private List<string> _uploadedFiles;
        private PrintRequestOptions _options;

        public void Arrange(bool batchWithCertificates = true)
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintRequestCommand>>();
            _mockPrintCreator = new Mock<IPrintCreator>();
            _mockBatchService = new Mock<IBatchService>();
            _mockScheduleService = new Mock<IScheduleService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockExternalFileTransferClient = new Mock<IExternalBlobFileTransferClient>();
            _mockInternalFileTransferClient = new Mock<IInternalBlobFileTransferClient>();
            _mockOptions = new Mock<IOptions<PrintRequestOptions>>();

            _options = new PrintRequestOptions
            {
                Directory = "MockPrintRequestDirectory",
                ArchiveDirectory = "MockArchivePrintRequestDirectory",
                AddReadyToPrintLimit = 50
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
                    .ReturnsAsync(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchNumber = _batchNumberWithCertificates.ToString(), BatchDate = DateTime.Now } }));
            }

            _mockExternalFileTransferClient
                .Setup(m => m.GetFileNames(It.IsAny<string>(), false))
                .ReturnsAsync(_uploadedFiles);

            _mockScheduleService
                .Setup(m => m.Get())
                .ReturnsAsync(new Schedule { Id = _scheduleId, RunTime = DateTime.Now });

            _certificates = Builder<CertificatePrintSummaryBase>
                .CreateListOfSize(10)
                .All()
                .Build() as List<CertificatePrintSummaryBase>;

            _mockPrintCreator
                .Setup(m => m.Create(It.IsAny<int>(), It.IsAny<List<CertificatePrintSummaryBase>>()))
                .Returns(new PrintOutput()
                {
                    Batch = Builder<BatchData>.CreateNew().Build(),
                    PrintData = Builder<PrintData>.CreateListOfSize(10).All().Build() as List<PrintData>
                });


            _batchWithCertificatesId = Guid.NewGuid();
            _batchWithCertificates = new Batch { Id = _batchWithCertificatesId, BatchNumber = _batchNumberWithCertificates, Certificates = _certificates };

            _batchWithoutCertificatesId = Guid.NewGuid();
            _batchWithoutCertificates = new Batch { Id = _batchWithoutCertificatesId, BatchNumber = _batchNumberWithoutCertificates, Certificates = new List<CertificatePrintSummaryBase>() };

            if (batchWithCertificates)
            {
                _mockBatchService
                    .Setup(m => m.Get(_batchNumberWithCertificates))
                    .ReturnsAsync(_batchWithCertificates);

                _mockBatchService
                    .Setup(m => m.GetCertificatesForBatchNumber(_batchNumberWithCertificates))
                    .ReturnsAsync(_batchWithCertificates.Certificates);

                _mockBatchService
                .Setup(m => m.BuildPrintBatchReadyToPrint(It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(_batchWithCertificates);
            }
            else
            {
                _mockBatchService
                    .Setup(m => m.Get(_batchNumberWithoutCertificates))
                    .ReturnsAsync(_batchWithoutCertificates);

                _mockBatchService
                    .Setup(m => m.GetCertificatesForBatchNumber(_batchNumberWithoutCertificates))
                    .ReturnsAsync(_batchWithoutCertificates.Certificates);

                _mockBatchService
                .Setup(m => m.BuildPrintBatchReadyToPrint(It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(_batchWithoutCertificates);
            }

            _sut = new Domain.Print.PrintRequestCommand(
            _mockLogger.Object,
            _mockPrintCreator.Object,
            _mockBatchService.Object,
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
            Arrange();
            var logMessage = "PrintRequestCommand - Started";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldLogIfNotScheduledToRun()
        {
            Arrange();
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            var logMessage = "PrintRequestCommand - There is no print schedule which allows printing at this time";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldNotCallTheNotificationServiceIfNotScheduledToRun()
        {
            Arrange();
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockNotificationService.Verify(m => m.SendPrintRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldNotChangeAnyDataIfNotScheduledToRun()
        {
            Arrange();
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockBatchService.Verify(m => m.Update(It.IsAny<Batch>()), Times.Never);
            _mockScheduleService.Verify(q => q.Save(It.IsAny<Schedule>()), Times.Never());
        }

        [Test]
        public async Task ThenItShouldNotCreateAJsonPrintOutputIfNotScheduledToRun()
        {
            Arrange();
            _mockScheduleService
                .Setup(m => m.Get())
                .Returns(Task.FromResult<Schedule>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockPrintCreator.Verify(m => m.Create(It.IsAny<int>(), It.IsAny<List<CertificatePrintSummary>>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldCompleteScheduleIfThereAreNoCertificatesToProcess()
        {
            Arrange(false);
            
            // Act
            await _sut.Execute();

            // Assert
            _mockScheduleService.Verify(q => q.Save(It.Is<Schedule>(s => s.Id ==_scheduleId)), Times.Once());

            _mockBatchService.Verify(m => m.Update(It.IsAny<Batch>()), Times.Never);
            _mockNotificationService.Verify(m => m.SendPrintRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldCompleteScheduleIfThereAreCertificatesToProcess()
        {
            Arrange();

            // Act
            await _sut.Execute();

            // Assert
            _mockScheduleService.Verify(q => q.Save(It.Is<Schedule>(s => s.Id == _scheduleId)), Times.Once());

            _mockBatchService.Verify(m => m.Update(It.IsAny<Batch>()), Times.Once);
            _mockNotificationService.Verify(m => m.SendPrintRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldCreateAJsonPrint()
        {
            Arrange();

            // Act
            await _sut.Execute();

            // Assert
            _mockPrintCreator.Verify(m => m.Create(_batchNumberWithCertificates, _certificates), Times.Once);
            _mockNotificationService.Verify(m => m.SendPrintRequest(_batchNumberWithCertificates, _certificates.Count, It.IsAny<string>()), Times.Once);
            
            _mockBatchService.Verify(m => m.Update(It.Is<Batch>(b => b.BatchNumber.Equals(_batchNumberWithCertificates))), Times.Once);
            _mockScheduleService.Verify(m => m.Save(It.Is<Schedule>(s => s.Id == _scheduleId)), Times.Once);
        }

        [Test]
        public async Task ThenItShouldUploadAndArchivePrintRequest()
        {
            Arrange();
            var fileName = $"PrintBatch-{_batchNumberWithCertificates.ToString().PadLeft(3, '0')}-{DateTime.UtcNow.UtcToTimeZoneTime():ddMMyyHHmm}.json";
            
            // Act
            await _sut.Execute();

            // Assert
            _mockExternalFileTransferClient.Verify(m => m.UploadFile(It.IsAny<string>(), It.Is<string>(s => s == $"{_options.Directory}/{fileName}")));
            _mockInternalFileTransferClient.Verify(m => m.UploadFile(It.IsAny<string>(), It.Is<string>(s => s == $"{_options.ArchiveDirectory}/{fileName}")));
        }
    }
}
