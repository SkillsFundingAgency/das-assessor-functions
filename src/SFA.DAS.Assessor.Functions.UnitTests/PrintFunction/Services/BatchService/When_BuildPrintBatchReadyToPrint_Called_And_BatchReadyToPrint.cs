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

            // Arrange
            Rearrange();
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
                .Setup(m => m.UpdateBatchLogReadyToPrintAddCertifictes(_batchNumber, It.IsAny<UpdateBatchLogReadyToPrintAddCertificatesRequest>()))
                .Callback((int batchNumber, UpdateBatchLogReadyToPrintAddCertificatesRequest request) =>
                {
                    _certifictesAddedReadyToPrint = _certificateReadyToPrintCount >= request.MaxCertificatesToBeAdded
                        ? request.MaxCertificatesToBeAdded
                        : _certificateReadyToPrintCount;

                    _certificateReadyToPrintCount -= _certifictesAddedReadyToPrint;
                    Rearrange(); // Ensure arranged values are updated in mocks
                })
                .ReturnsAsync((int batchNumber, UpdateBatchLogReadyToPrintAddCertificatesRequest request) => 
                {
                    return _certifictesAddedReadyToPrint;
                });
        }

        [Test]
        public async Task Then_AssessorApiCalled_ToGetBatchNumberReadyToPrint()
        {
            // Arrange
            _certificateReadyToPrintCount = 110;

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, 50);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetBatchNumberReadyToPrint(), Times.Once);
            result.Should().Equals(_batchNumber);
        }

        [Test]
        public async Task Then_AssessorApiCalled_ToGetCertificatesReadyToPrintCount()
        {
            // Arrange
            _certificateReadyToPrintCount = 110;

            // Act
            var result = await _sut.BuildPrintBatchReadyToPrint(_scheduledDateTime, 50);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetCertificatesReadyToPrintCount(), Times.Exactly(4));
            result.Should().Equals(_batchNumber);
        }
    }
}
