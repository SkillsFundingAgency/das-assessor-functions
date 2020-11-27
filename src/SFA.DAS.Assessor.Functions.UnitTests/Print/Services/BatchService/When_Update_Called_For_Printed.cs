using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Services.BatchService
{
    public class When_Update_Called_For_Printed : BatchServiceTestBase
    {
        private Batch _batch;
        private UpdateBatchLogSentToPrinterRequest _request;
        private Mock<ICollector<string>> _mockStorageQueue;

        [SetUp]
        public override void Arrange()
        {
            // Arrange
            base.Arrange();

            _batch = Builder<Batch>.CreateNew().Build();
            _batch.BatchNumber = _batchNumber;
            _batch.Certificates = Builder<Certificate>.CreateListOfSize(13).All().Build() as List<Certificate>;
            _batch.Status = CertificateStatus.Printed;

            _request = new UpdateBatchLogSentToPrinterRequest()
            {
                BatchCreated = _batch.BatchCreated,
                NumberOfCertificates = _batch.NumberOfCertificates,
                NumberOfCoverLetters = _batch.NumberOfCoverLetters,
                CertificatesFileName = _batch.CertificatesFileName,
                FileUploadStartTime = _batch.FileUploadStartTime,
                FileUploadEndTime = _batch.FileUploadEndTime,
            };

            _mockStorageQueue = new Mock<ICollector<string>>();
        }

        [Test]
        public async Task Then_AssessorApiCalled_UpdateBatchLogPrinted()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.UpdateBatchLogPrinted(_batchNumber, It.IsAny<UpdateBatchLogPrintedRequest>()))
                .ReturnsAsync(_validResponse);

            // Act
            await _sut.Update(_batch);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.UpdateBatchLogPrinted(_batchNumber, It.IsAny<UpdateBatchLogPrintedRequest>()), Times.Once);
        }

        [Test]
        public async Task Then_CertificateUpdateMessages_AreCreatedAfterValidReponse()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.UpdateBatchLogPrinted(_batchNumber, It.IsAny<UpdateBatchLogPrintedRequest>()))
                .ReturnsAsync(_validResponse);

            // Act
            var printStatusUpdateMessages = await _sut.Update(_batch);

            // Assert
            printStatusUpdateMessages.Count.Should().Be(_batch.Certificates.Count);
        }

        [Test]
        public async Task Then_CertificateUpdateMessages_AreNotCreatedAfterInvalidReponse()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.UpdateBatchLogPrinted(_batchNumber, It.IsAny<UpdateBatchLogPrintedRequest>()))
                .ReturnsAsync(_invalidResponse);

            // Act
            var printStatusUpdateMessages = await _sut.Update(_batch);

            // Assert
            printStatusUpdateMessages.Count.Should().Be(0);
        }
    }
}
