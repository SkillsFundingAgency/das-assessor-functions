using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintStatusUpdateFunction
{
    public class When_Run_Called
    {
        private Functions.Print.PrintStatusUpdateFunction _sut;
        
        private Mock<ILogger<Functions.Print.PrintStatusUpdateFunction>> _mockLogger;
        private Mock<IPrintStatusUpdateCommand> _mockCommand;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Functions.Print.PrintStatusUpdateFunction>>();
            _mockCommand = new Mock<IPrintStatusUpdateCommand>();

            _sut = new Functions.Print.PrintStatusUpdateFunction(_mockCommand.Object, _mockLogger.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            var message = new CertificatePrintStatusUpdateMessage();

            // Act - TimerSchedule is not used so null allowed
            await _sut.Run(message);

            // Assert
            _mockCommand.Verify(p => p.Execute(message), Times.Once());
        }
    }
}
