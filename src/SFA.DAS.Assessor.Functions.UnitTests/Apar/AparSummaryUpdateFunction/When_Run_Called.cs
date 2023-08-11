using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Apar.AparSummaryUpdateFunction
{
    public class When_Run_Called
    {
        private Functions.Apar.AparSummaryUpdateFunction _sut;
        private Mock<ILogger> _logger;
        private Mock<IAparSummaryUpdateCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger>();
            _command = new Mock<IAparSummaryUpdateCommand>();

            _sut = new Functions.Apar.AparSummaryUpdateFunction(_command.Object);
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
