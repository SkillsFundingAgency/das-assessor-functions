using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.DatabaseMaintenance
{
    public class When_Run_Called
    {
        private Functions.DatabaseMaintenance.DatabaseMaintenanceFunction _sut;
        private Mock<ILogger<Functions.DatabaseMaintenance.DatabaseMaintenanceFunction>> _logger;
        private Mock<IDatabaseMaintenanceCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger<Functions.DatabaseMaintenance.DatabaseMaintenanceFunction>>();
            _command = new Mock<IDatabaseMaintenanceCommand>();

            _sut = new Functions.DatabaseMaintenance.DatabaseMaintenanceFunction(_command.Object, _logger.Object);
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
