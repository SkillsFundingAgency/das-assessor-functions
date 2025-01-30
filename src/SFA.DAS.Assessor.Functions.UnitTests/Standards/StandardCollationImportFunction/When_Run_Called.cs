using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Standards.StandardImportFlow
{
    public class When_Run_Called
    {
        private Functions.Standards.StandardImportFunction _sut;
        private Mock<ILogger<Functions.Standards.StandardImportFunction>> _logger;
        private Mock<IStandardImportCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger<Functions.Standards.StandardImportFunction>>();
            _command = new Mock<IStandardImportCommand>();

            _sut = new Functions.Standards.StandardImportFunction(_command.Object, _logger.Object);
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
