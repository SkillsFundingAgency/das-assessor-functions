using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Apar.AparSummaryUpdateCommand
{
    public class When_Execute_Called
    {
        private Domain.Assessors.AparSummaryUpdateCommand _sut;
        private Mock<IAssessorServiceApiClient> _assessorServiceApiClient;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.Assessors.AparSummaryUpdateCommand>>();
            _assessorServiceApiClient = new Mock<IAssessorServiceApiClient>();

            _sut = new Domain.Assessors.AparSummaryUpdateCommand(logger.Object, _assessorServiceApiClient.Object);
        }

        [Test]
        public async Task Then_AparSummaryUpdate_ShouldBeCalled()
        {
            // Act
            await _sut.Execute();

            // Assert
            _assessorServiceApiClient.Verify(p => p.AparSummaryUpdate(), Times.Once());
        }
    }
}
