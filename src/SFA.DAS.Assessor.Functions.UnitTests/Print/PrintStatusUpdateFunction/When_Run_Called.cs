using Microsoft.Azure.WebJobs;
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

        private Mock<ILogger> _mockLogger;
        private Mock<IPrintStatusUpdateCommand> _mockCommand;
        private Mock<ICollector<CertificatePrintStatusUpdateErrorMessage>> _mockCollector;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger>();
            _mockCommand = new Mock<IPrintStatusUpdateCommand>();
            _mockCollector = new Mock<ICollector<CertificatePrintStatusUpdateErrorMessage>>();

            _sut = new Functions.Print.PrintStatusUpdateFunction(_mockCommand.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            var message = new CertificatePrintStatusUpdateMessage();

            // Act - TimerSchedule is not used so null allowed
            await _sut.Run(message, _mockCollector.Object, _mockLogger.Object);

            // Assert
            _mockCommand.Verify(p => p.Execute(message), Times.Once());
        }
    }
}
