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
    public class When_Update_Called_For_SentToPrinter : BatchServiceTestBase
    {
        private Batch _batch;

        [SetUp]
        public override void Arrange()
        {
            // Arrange
            base.Arrange();

            _batch = Builder<Batch>.CreateNew().Build();
            _batch.BatchNumber = _batchNumber;
            _batch.Certificates = Builder<CertificatePrintSummaryBase>.CreateListOfSize(13).All().Build() as List<CertificatePrintSummaryBase>;
            _batch.Status = CertificateStatus.SentToPrinter;
        }

        [Test]
        public async Task Then_AssessorApiCalled_UpdateBatchLogSentToPrinter()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.UpdateBatchLogSentToPrinter(_batchNumber, It.IsAny<UpdateBatchLogSentToPrinterRequest>()))
                .ReturnsAsync(_validResponse);

            // Act
            await _sut.Update(_batch);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.UpdateBatchLogSentToPrinter(_batchNumber, It.IsAny<UpdateBatchLogSentToPrinterRequest>()), Times.Once);

        }

        [Test]
        public async Task Then_CertificateUpdateMessages_AreCreatedAfterValidReponse()
        {
            // Arrange
            _mockAssessorServiceApiClient
                .Setup(m => m.UpdateBatchLogSentToPrinter(_batchNumber, It.IsAny<UpdateBatchLogSentToPrinterRequest>()))
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
                .Setup(m => m.UpdateBatchLogSentToPrinter(_batchNumber, It.IsAny<UpdateBatchLogSentToPrinterRequest>()))
                .ReturnsAsync(_invalidResponse);

            // Act
            var printStatusUpdateMessages = await _sut.Update(_batch);

            // Assert
            printStatusUpdateMessages.Count.Should().Be(0);
        }
    }
}
