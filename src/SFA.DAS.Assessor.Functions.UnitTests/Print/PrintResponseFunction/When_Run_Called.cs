using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintResponseFunction
{
    public class When_Run_Called
    {
        private Functions.Print.PrintResponseFunction _sut;

        private Mock<ILogger> _mockLogger;
        private Mock<IPrintResponseCommand> _mockCommand;
        private Mock<ICollector<CertificatePrintStatusUpdateMessage>> _mockCollector;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger>();
            _mockCommand = new Mock<IPrintResponseCommand>();
            _mockCollector = new Mock<ICollector<CertificatePrintStatusUpdateMessage>>();

            _sut = new Functions.Print.PrintResponseFunction(_mockCommand.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            // Act - TimerSchedule is not used so null allowed
            await _sut.Run(new TimerInfo(default, default, false), _mockCollector.Object, _mockLogger.Object);

            // Assert
            _mockCommand.Verify(p => p.Execute(), Times.Once());
        }
    }
}
