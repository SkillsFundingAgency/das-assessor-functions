using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Assessments.AssessmentsSummaryUpdateCommand
{
    public class When_Execute_Called
    {
        private Domain.Assessments.AssessmentsSummaryUpdateCommand _sut;
        private Mock<IAssessorServiceApiClient> _assessorServiceApiClient;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.Assessments.AssessmentsSummaryUpdateCommand>>();
            _assessorServiceApiClient = new Mock<IAssessorServiceApiClient>();

            _sut = new Domain.Assessments.AssessmentsSummaryUpdateCommand(logger.Object, _assessorServiceApiClient.Object);
        }

        [Test]
        public async Task ThenItShouldUpdateAssessmentsSummary()
        {
            // Act
            await _sut.Execute();

            // Assert
            _assessorServiceApiClient.Verify(p => p.UpdateAssessmentsSummary(), Times.Once());
        }
    }
}
