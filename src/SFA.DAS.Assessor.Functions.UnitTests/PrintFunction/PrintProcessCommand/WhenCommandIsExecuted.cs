
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
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
        private Mock<IAssessorServiceApiClient> _mockAssessorServiceApiClient;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IFileTransferClient> _mockFileTransferClient;
        private Mock<IOptions<SftpSettings>> _mockSftpSettings;

        private int _batchNumber = 1;
        private Guid _scheduleId = Guid.NewGuid();
        private List<CertificateResponse> _certificateResponses;
        private List<string> _downloadedFiles;
        private BatchLogResponse _batchLogResponse;
        private Guid _batchLogResponseId;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintProcessCommand>>();
            _mockPrintingJsonCreator = new Mock<IPrintingJsonCreator>();
            _mockPrintingSpreadsheetCreator = new Mock<IPrintingSpreadsheetCreator>();
            _mockAssessorServiceApiClient = new Mock<IAssessorServiceApiClient>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockFileTransferClient = new Mock<IFileTransferClient>();
            _mockSftpSettings = new Mock<IOptions<SftpSettings>>();

            _downloadedFiles = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < 10; i++)
            {
                var filename = $"printResponse-0120-{generator.Next(111111, 999999)}.json";
                _downloadedFiles.Add(filename);

                _mockFileTransferClient
                    .Setup(m => m.DownloadFile(filename))
                    .Returns(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchNumber = _batchNumber.ToString(), BatchDate = DateTime.Now } }));
            };

            _mockFileTransferClient
                .Setup(m => m.GetListOfDownloadedFiles())
                .ReturnsAsync(_downloadedFiles);

            _mockAssessorServiceApiClient
                .Setup(m => m.GetSchedule(ScheduleType.PrintRun))
                .ReturnsAsync(new ScheduleRun { Id = _scheduleId });

            _mockAssessorServiceApiClient
                .Setup(m => m.GetCurrentBatchLog())
                .ReturnsAsync(new BatchLogResponse { Id = Guid.NewGuid(), BatchNumber = _batchNumber });

            _certificateResponses = Builder<CertificateResponse>
                .CreateListOfSize(10)
                .All()
                .With(o => o.CertificateData = Builder<CertificateDataResponse>.CreateNew().Build()) 
                .Build() as List<CertificateResponse>;

            _mockAssessorServiceApiClient
                .Setup(m => m.GetCertificatesToBePrinted())
                .ReturnsAsync(_certificateResponses);

            _batchLogResponseId = Guid.NewGuid();
            _batchLogResponse = new BatchLogResponse { Id = _batchLogResponseId, BatchNumber = _batchNumber };
            _mockAssessorServiceApiClient
                .Setup(m => m.GetGetBatchLogByBatchNumber(_batchNumber.ToString()))
                .ReturnsAsync(_batchLogResponse);

            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(new SftpSettings { UseJson = true });

            _sut = new Domain.Print.PrintProcessCommand(
                _mockLogger.Object,
                _mockPrintingJsonCreator.Object,
                _mockPrintingSpreadsheetCreator.Object,
                _mockAssessorServiceApiClient.Object,
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
            _mockAssessorServiceApiClient
                .Setup(m => m.GetSchedule(ScheduleType.PrintRun))
                .Returns(Task.FromResult<ScheduleRun>(null));

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
            _mockAssessorServiceApiClient
                .Setup(m => m.GetSchedule(ScheduleType.PrintRun))
                .Returns(Task.FromResult<ScheduleRun>(null));

            // Act
            await _sut.Execute();

            // Assert
            _mockNotificationService.Verify(m => m.Send(It.IsAny<int>(), It.IsAny<List<CertificateResponse>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldNotChangeAnyDataViaAssessorEndpointsIfNotScheduledToRun()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.GetSchedule(ScheduleType.PrintRun))
                .Returns(Task.FromResult<ScheduleRun>(null));

            // Act
            await _sut.Execute();

            // Assert            
            _mockAssessorServiceApiClient.Verify(m => m.UpdateBatchDataInBatchLog(It.IsNotIn(_batchLogResponseId), It.IsAny<BatchData>()), Times.Never);
            _mockAssessorServiceApiClient.Verify(m => m.CreateBatchLog(It.IsAny<CreateBatchLogRequest>()), Times.Never);
            _mockAssessorServiceApiClient.Verify(m => m.ChangeStatusToPrinted(It.IsAny<int>(), It.IsAny<IEnumerable<CertificateResponse>>()), Times.Never);
            _mockAssessorServiceApiClient.Verify(q => q.CompleteSchedule(It.IsAny<Guid>()), Times.Never());
        }

        [Test]
        public async Task ThenItShouldNotCreateASpreadSheetIfNotScheduledToRun()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.GetSchedule(ScheduleType.PrintRun))
                .Returns(Task.FromResult<ScheduleRun>(null));

            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(new SftpSettings { UseJson = false });

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingSpreadsheetCreator.Verify(m => m.Create(It.IsAny<int>(), It.IsAny<IEnumerable<CertificateResponse>>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldNotCreateAJsonPrintOutputIfNotScheduledToRun()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.GetSchedule(ScheduleType.PrintRun))
                .Returns(Task.FromResult<ScheduleRun>(null));

            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(new SftpSettings { UseJson = true });

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingJsonCreator.Verify(m => m.Create(It.IsAny<int>(), It.IsAny<List<CertificateResponse>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoCertificatesToProcess()
        {
            // Arrange
            var logMessage = "No certificates to process";

            _mockAssessorServiceApiClient
                .Setup(m => m.GetCertificatesToBePrinted())
                .ReturnsAsync(new List<CertificateResponse>());

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldOnlyCompleteScheduleIfThereAreNoCertificatesToProcess()
        {
            // Arrange
            _mockAssessorServiceApiClient
               .Setup(m => m.GetCertificatesToBePrinted())
               .ReturnsAsync(new List<CertificateResponse>());

            // Act
            await _sut.Execute();

            // Assert            
            _mockAssessorServiceApiClient.Verify(q => q.CompleteSchedule(_scheduleId), Times.Once());

            _mockAssessorServiceApiClient.Verify(m => m.UpdateBatchDataInBatchLog(It.IsNotIn(_batchLogResponseId), It.IsAny<BatchData>()), Times.Never);
            _mockAssessorServiceApiClient.Verify(m => m.CreateBatchLog(It.IsAny<CreateBatchLogRequest>()), Times.Never);
            _mockAssessorServiceApiClient.Verify(m => m.ChangeStatusToPrinted(It.IsAny<int>(), It.IsAny<IEnumerable<CertificateResponse>>()), Times.Never);            
        }

        [Test]
        public async Task ThenItShouldLogIfThereAreNoCertificateResponsesToProcess()
        {
            // Arrange
            var logMessage = "No certificate responses to process";

            _mockFileTransferClient
                .Setup(m => m.GetListOfDownloadedFiles())
                .ReturnsAsync(new List<string>());

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task ThenItShouldCreateASpreadSheetIfConfiguredTo()
        {
            // Arrange
            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(new SftpSettings { UseJson = false });

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingSpreadsheetCreator.Verify(m => m.Create(_batchNumber + 1, _certificateResponses), Times.Once);
        }

        [Test]
        public async Task ThenItShouldCreateAJsonPrintIfConfiguredTo()
        {
            // Arrange
            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(new SftpSettings { UseJson = true });

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingJsonCreator.Verify(m => m.Create(_batchNumber + 1, _certificateResponses, It.IsAny<string>()), Times.Once);
            _mockNotificationService.Verify(m => m.Send(_batchNumber + 1, _certificateResponses, It.IsAny<string>()), Times.Once);
            _mockFileTransferClient.Verify(m => m.LogUploadDirectory(), Times.Once);
            _mockAssessorServiceApiClient.Verify(m => m.CreateBatchLog(It.Is<CreateBatchLogRequest>(r => r.BatchNumber.Equals(_batchNumber + 1))), Times.Once);
            _mockAssessorServiceApiClient.Verify(m => m.ChangeStatusToPrinted(_batchNumber + 1, _certificateResponses), Times.Once);
            _mockAssessorServiceApiClient.Verify(m => m.CompleteSchedule(_scheduleId), Times.Once);
        }

        [Test]
        public async Task ThenItShouldCreateASpreadSheetPrintIfConfiguredTo()
        {
            // Arrange
            _mockSftpSettings
                .Setup(m => m.Value)
                .Returns(new SftpSettings { UseJson = false });

            // Act
            await _sut.Execute();

            // Assert            
            _mockPrintingSpreadsheetCreator.Verify(m => m.Create(_batchNumber + 1, _certificateResponses), Times.Once);
            _mockNotificationService.Verify(m => m.Send(_batchNumber + 1, _certificateResponses, It.IsAny<string>()), Times.Once);
            _mockFileTransferClient.Verify(m => m.LogUploadDirectory(), Times.Once);
            _mockAssessorServiceApiClient.Verify(m => m.CreateBatchLog(It.Is<CreateBatchLogRequest>(r => r.BatchNumber.Equals(_batchNumber + 1))), Times.Once);
            _mockAssessorServiceApiClient.Verify(m => m.ChangeStatusToPrinted(_batchNumber + 1, _certificateResponses), Times.Once);
            _mockAssessorServiceApiClient.Verify(m => m.CompleteSchedule(_scheduleId), Times.Once);
        }

        [Test]
        public async Task ThenItShouldGenerateABatchNumber()
        {
            // Arrange

            // Act
            await _sut.Execute();

            // Assert
            _mockAssessorServiceApiClient.Verify(q => q.GetCurrentBatchLog(), Times.Once());
        }

        [Test]
        public async Task ThenItShouldLogIfUnableToDownloadFileToDelete()
        {
            // Arrange
            _mockFileTransferClient
                .Setup(m => m.DownloadFile(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new BatchResponse { Batch = null }));

            var logMessage = "Could not process downloaded file to correct format";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(_downloadedFiles.Count));
        }

        [Test]
        public async Task ThenItShouldLogIfUnableToDownloadFileToDeleteDueToInvalidBatchDate()
        {
            // Arrange
            _mockFileTransferClient
                .Setup(m => m.DownloadFile(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchDate = DateTime.MinValue } }));

            var logMessage = "Could not process downloaded file to correct format";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(_downloadedFiles.Count));
        }

        [Test]
        public async Task ThenItShouldNotProcessTheFileIfFormatNotRecognised()
        {
            // Arrange
            _mockFileTransferClient
                .Setup(m => m.DownloadFile(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchDate = DateTime.MinValue } }));

            // Act
            await _sut.Execute();

            // Assert
            _mockAssessorServiceApiClient.Verify(m => m.UpdateBatchDataInBatchLog(It.IsAny<Guid>(), It.IsAny<BatchData>()), Times.Never);
            _mockFileTransferClient.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenItShouldLogIfAnExistingBatchNumberCouldNotBeMatched()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.GetGetBatchLogByBatchNumber(It.IsAny<string>()))
                .ReturnsAsync(new BatchLogResponse { Id = null });

            var logMessage = "Could not match an existing batch Log Batch Number";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(_downloadedFiles.Count));
        }

        [Test]
        public async Task ThenItShouldLogIftheBatchNumberIsNotanInteger()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.GetGetBatchLogByBatchNumber(It.IsAny<string>()))
                .ReturnsAsync(new BatchLogResponse { Id = _batchLogResponseId, BatchNumber = _batchNumber });

            _mockFileTransferClient
                 .Setup(m => m.DownloadFile(It.IsAny<string>()))
                 .Returns(JsonConvert.SerializeObject(new BatchResponse { Batch = new BatchDataResponse { BatchNumber = "test", BatchDate = DateTime.Now } }));

            var logMessage = "The Batch Number is not an integer [test]";

            // Act
            await _sut.Execute();

            // Assert
            _mockLogger.Verify(m => m.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(_downloadedFiles.Count));
        }

        [Test]
        public async Task ThenItShouldProcessAndDeleteDownloadedFiles()
        {
            // Arrange

            // Act
            await _sut.Execute();

            // Assert
            _mockAssessorServiceApiClient.Verify(m => m.UpdateBatchDataInBatchLog(_batchLogResponseId, It.Is<BatchData>(b => b.BatchNumber.Equals(_batchNumber))), Times.Exactly(_downloadedFiles.Count));
            _mockFileTransferClient.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Exactly(_downloadedFiles.Count));
        }

    }
}
