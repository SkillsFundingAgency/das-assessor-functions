using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Assessors.AssessmentOrganisationsListUpdateFunction
{
    public class When_Run_Called
    {
        private Functions.Assessors.AssessmentOrganisationsListUpdateFunction _sut;
        private Mock<ILogger> _logger;
        private Mock<IAssessmentOrganisationsListUpdateCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger>();
            _command = new Mock<IAssessmentOrganisationsListUpdateCommand>();

            _sut = new Functions.Assessors.AssessmentOrganisationsListUpdateFunction(_command.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            // Act - TimerSchedule is not used so null allowed
            await _sut.Run(new TimerInfo(default, default, false), _logger.Object);

            // Assert
            _command.Verify(p => p.Execute(), Times.Once());
        }
    }
}
