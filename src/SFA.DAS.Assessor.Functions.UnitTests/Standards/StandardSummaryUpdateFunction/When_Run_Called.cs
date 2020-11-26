using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Standards.StandardSummaryUpdateFlow
{
    public class When_Run_Called
    {
        private Functions.Standards.StandardSummaryUpdateFunction _sut;
        private Mock<ILogger> _logger;
        private Mock<IStandardSummaryUpdateCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger>();
            _command = new Mock<IStandardSummaryUpdateCommand>();

            _sut = new Functions.Standards.StandardSummaryUpdateFunction(_command.Object);
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
