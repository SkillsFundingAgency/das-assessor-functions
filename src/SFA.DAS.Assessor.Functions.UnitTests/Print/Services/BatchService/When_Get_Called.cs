using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Services.BatchService
{
    public class When_Get_Called : BatchServiceTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_AssessorApiCalled_ToGetBatch()
        {
            // Arrange
            var response = Builder<BatchLogResponse>.CreateNew().Build();
            response.BatchNumber = _batchNumber;

            _mockAssessorServiceApiClient
                .Setup(m => m.GetBatchLog(_batchNumber))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.Get(_batchNumber);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.GetBatchLog(_batchNumber), Times.Once);
            result.Should().Equals(response);
        }
    }
}
