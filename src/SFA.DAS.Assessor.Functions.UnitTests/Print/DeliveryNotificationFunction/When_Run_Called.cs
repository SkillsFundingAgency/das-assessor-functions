using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.DeliveryNotificationFunction
{
    public class When_Run_Called
    {
        private Functions.Print.DeliveryNotificationFunction _sut;
        
        private Mock<ILogger<Functions.Print.DeliveryNotificationFunction>> _mockLogger;
        private Mock<IDeliveryNotificationCommand> _mockCommand;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Functions.Print.DeliveryNotificationFunction>>();
            _mockCommand = new Mock<IDeliveryNotificationCommand>();

            _sut = new Functions.Print.DeliveryNotificationFunction(_mockCommand.Object, _mockLogger.Object);
        }

        [Test]
        public async Task ThenItShouldExecuteCommand()
        {
            // Act
            TimerInfo timerInfo = TimerInfoFactory.Create();
            await _sut.Run(timerInfo);

            // Assert
            _mockCommand.Verify(p => p.Execute(), Times.Once());
        }
    }
}
