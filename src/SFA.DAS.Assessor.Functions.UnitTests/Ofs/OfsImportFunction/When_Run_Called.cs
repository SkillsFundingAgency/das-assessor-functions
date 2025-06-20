using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofs.OfsImportFunction
{
    public class When_Run_Called
    {
        private Functions.Ofs.OfsImportFunction _sut;
        private Mock<ILogger<Functions.Ofs.OfsImportFunction>> _logger;
        private Mock<IOfsImportCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger<Functions.Ofs.OfsImportFunction>>();
            _command = new Mock<IOfsImportCommand>();

            _sut = new Functions.Ofs.OfsImportFunction(_command.Object, _logger.Object);
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
