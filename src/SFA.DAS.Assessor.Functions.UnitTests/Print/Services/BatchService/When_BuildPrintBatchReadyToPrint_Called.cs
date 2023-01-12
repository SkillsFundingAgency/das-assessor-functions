using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Services.BatchService
{
    public class When_BuildPrintBatchReadyToPrint_Called : BatchServiceTestBase
    {
        DateTime _scheduledDateTime = DateTime.UtcNow;

        private int _certificateReadyToPrintCount = 0;
        private int _certifictesAddedReadyToPrint = 0;

        public void Arrange(int certificateReadyToPrintCount)
        {
            base.Arrange();

            Rearrange(certificateReadyToPrintCount);

            _mockAssessorServiceApiClient
                .Setup(m => m.GetBatchLog(_batchNumber))
                .ReturnsAsync(new BatchLogResponse
                {
                    Id = Guid.NewGuid(),
                    BatchNumber = _batchNumber
                });

            var certificates = certificateReadyToPrintCount > 0
                ? Builder<CertificatePrintSummary>.CreateListOfSize(certificateReadyToPrintCount).Build() as List<CertificatePrintSummary>
                : new List<CertificatePrintSummary>();

            _mockAssessorServiceApiClient
                .Setup(m => m.GetCertificatesForBatchNumber(_batchNumber))
                .ReturnsAsync(new CertificatesForBatchNumberResponse
                {
                    Certificates = certificates
                });
        }

        private void Rearrange(int certificateReadyToPrintCount)
        {
            _mockAssessorServiceApiClient
                .Setup(m => m.GetBatchNumberReadyToPrint())
                .ReturnsAsync(_batchNumber);

            _mockAssessorServiceApiClient
                .Setup(m => m.GetCertificatesReadyToPrintCount())
                .ReturnsAsync(_certificateReadyToPrintCount);

            _mockAssessorServiceApiClient
                .Setup(m => m.UpdateBatchLogReadyToPrintAddCertifictes(_batchNumber, It.IsAny<int>()))
                .Callback((int batchNumber, int maxCertificatesToBeAdded) =>
                {
                    _certifictesAddedReadyToPrint = _certificateReadyToPrintCount >= maxCertificatesToBeAdded
                        ? maxCertificatesToBeAdded
                        : _certificateReadyToPrintCount;

                    _certificateReadyToPrintCount -= _certifictesAddedReadyToPrint;
                    Rearrange(certificateReadyToPrintCount); // Ensure arranged values are updated in mocks
                })
                .ReturnsAsync((int batchNumber, int maxCertificatesToBeAdded) =>
                {
                    return _certifictesAddedReadyToPrint;
                });

            _mockAssessorServiceApiClient
                .Setup(m => m.GetBatchNumberReadyToPrint())
                .ReturnsAsync(_batchNumber);
        }

        [TestCase(110, 50)]
        [TestCase(10, 30)]
        [TestCase(0, 50)]
        public async Task Then_AssessorApiCalled_ToGetBatchNumberReadyToPrint(
            int certificateReadyToPrintCount,
            int maxCertificatesToAdd)
        {
            // Arrange
            _certificateReadyToPrintCount = certificateReadyToPrintCount;
            Arrange(certificateReadyToPrintCount);

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, maxCertificatesToAdd);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetBatchNumberReadyToPrint(), Times.Once);
        }

        [TestCase(110, 50, 4)]
        [TestCase(110, 15, 9)]
        [TestCase(0, 15, 1)]
        public async Task Then_AssessorApiCalled_ToGetCertificatesReadyToPrintCount(
            int certificateReadyToPrintCount,
            int maxCertificatesToAdd,
            int verifyReadyToPrintCount)
        {
            // Arrange
            _certificateReadyToPrintCount = certificateReadyToPrintCount;
            Arrange(certificateReadyToPrintCount);

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, maxCertificatesToAdd);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetCertificatesReadyToPrintCount(), Times.Exactly(verifyReadyToPrintCount));
        }

        [TestCase(100, 50)]
        public async Task Then_AssessorApiCalled_ToGetBatchLog_And_Certificates(
            int certificateReadyToPrintCount,
            int maxCertificatesToAdd)
        {
            // Arrange
            _certificateReadyToPrintCount = certificateReadyToPrintCount;
            Arrange(certificateReadyToPrintCount);

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, maxCertificatesToAdd);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetBatchLog(_batchNumber), Times.Once);
            _mockAssessorServiceApiClient.Verify(v => v.GetCertificatesForBatchNumber(_batchNumber), Times.Once);
            result.BatchNumber.Should().Be(_batchNumber);
            result.Certificates.Count.Should().Be(certificateReadyToPrintCount);
        }
    }
}
