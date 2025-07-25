using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Standards.StandardSummaryUpdateFlow
{
    public class When_Run_Called
    {
        private Functions.Standards.StandardSummaryUpdateFunction _sut;
        private Mock<ILogger<Functions.Standards.StandardSummaryUpdateFunction>> _logger;
        private Mock<IStandardSummaryUpdateCommand> _command;

        [SetUp]
        public void Arrange()
        {
            _logger = new Mock<ILogger<Functions.Standards.StandardSummaryUpdateFunction>>();
            _command = new Mock<IStandardSummaryUpdateCommand>();

            _sut = new Functions.Standards.StandardSummaryUpdateFunction(_command.Object, _logger.Object);
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
