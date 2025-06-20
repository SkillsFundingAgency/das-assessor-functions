using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintResponseFunction
{
    public class When_Run_Called
    {
        private Functions.Print.PrintResponseFunction _sut;
        
        private Mock<ILogger<Functions.Print.PrintResponseFunction>> _mockLogger;
        private Mock<IPrintResponseCommand> _mockCommand;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Functions.Print.PrintResponseFunction>>();
            _mockCommand = new Mock<IPrintResponseCommand>();

            _sut = new Functions.Print.PrintResponseFunction(_mockCommand.Object, _mockLogger.Object);
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
