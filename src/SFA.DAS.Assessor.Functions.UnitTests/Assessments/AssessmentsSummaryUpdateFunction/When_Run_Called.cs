using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Assessments.Interfaces;
using SFA.DAS.Assessor.Functions.Functions.Assessments;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Assessments.AssessmentSummaryUpdateFlow
{
    public class When_Run_Called
    {
        private Functions.Assessments.AssessmentsSummaryUpdateFunction _sut;
        private Mock<IAssessmentsSummaryUpdateCommand> _command;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<AssessmentsSummaryUpdateFunction>>();
            _command = new Mock<IAssessmentsSummaryUpdateCommand>();

            _sut = new Functions.Assessments.AssessmentsSummaryUpdateFunction(_command.Object, logger.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            // Act
            TimerInfo timerInfo = TimerInfoFactory.Create();
            await _sut.Run(timerInfo);

            // Assert
            _command.Verify(p => p.Execute(), Times.Once());
        }
    }
}
