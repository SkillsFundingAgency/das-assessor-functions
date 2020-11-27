using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintStatusUpdateFunction
{
    public class When_Run_Called
    {
        private Functions.Print.PrintStatusUpdateFunction _sut;
        
        private Mock<ILogger> _mockLogger;
        private Mock<IPrintStatusUpdateCommand> _mockCommand;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger>();
            _mockCommand = new Mock<IPrintStatusUpdateCommand>();

            _sut = new Functions.Print.PrintStatusUpdateFunction(_mockCommand.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            var message = "{}";

            // Act - TimerSchedule is not used so null allowed
            await _sut.Run(message, _mockLogger.Object);

            // Assert
            _mockCommand.Verify(p => p.Execute(message), Times.Once());
        }
    }
}
