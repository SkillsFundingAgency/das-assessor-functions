using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.Services.BatchService
{
    public class When_BuildPrintBatchReadyToPrint_Called_And_BatchReadyToPrint : BatchServiceTestBase
    {
        DateTime _scheduledDateTime = DateTime.UtcNow;
        
        private int _certificateReadyToPrintCount = 0;
        private int _certifictesAddedReadyToPrint = 0;

        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }
        private void Rearrange()
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
                    Rearrange(); // Ensure arranged values are updated in mocks
                })
                .ReturnsAsync((int batchNumber, int maxCertificatesToBeAdded) => 
                {
                    return _certifictesAddedReadyToPrint;
                });
        }

        [TestCase(110, 50)]
        public async Task Then_AssessorApiCalled_ToGetBatchNumberReadyToPrint(
            int certificateReadyToPrintCount, 
            int maxCertificatesToAdd)
        {
            // Arrange
            _certificateReadyToPrintCount = certificateReadyToPrintCount;
            Rearrange();

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, maxCertificatesToAdd);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetBatchNumberReadyToPrint(), Times.Once);
            result.Should().Equals(_batchNumber);
        }

        [TestCase(110, 50, 4)]
        [TestCase(110, 15, 9)]
        public async Task Then_AssessorApiCalled_ToGetCertificatesReadyToPrintCount(
            int certificateReadyToPrintCount,
            int maxCertificatesToAdd,
            int verifyReadyToPrintCount)
        {
            // Arrange
            _certificateReadyToPrintCount = certificateReadyToPrintCount;
            Rearrange();

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, maxCertificatesToAdd);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetCertificatesReadyToPrintCount(), Times.Exactly(verifyReadyToPrintCount));
            result.Should().Equals(_batchNumber);
        }
    }
}
