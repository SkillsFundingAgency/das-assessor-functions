using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;

namespace SFA.DAS.Assessor.Functions.UnitTests.Standards.StandardSummaryUpdateCommand
{
    public class WhenCommandIsExecuted
    {
        private Domain.Standards.StandardSummaryUpdateCommand _sut;
        private Mock<IAssessorServiceApiClient> _assessorServiceApiClient;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.Standards.StandardSummaryUpdateCommand>>();
            _assessorServiceApiClient = new Mock<IAssessorServiceApiClient>();

            _sut = new Domain.Standards.StandardSummaryUpdateCommand(logger.Object, _assessorServiceApiClient.Object);
        }

        [Test]
        public void ThenItShouldUpdateStandardSummary()
        {
            // Act
            _sut.Execute();

            // Assert
            _assessorServiceApiClient.Verify(p => p.UpdateStandardSummary(), Times.Once());
        }
    }
}
