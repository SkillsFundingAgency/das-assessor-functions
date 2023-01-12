using FizzWare.NBuilder;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintRequestFunction
{
    public class When_Run_Called
    {
        private Functions.Print.PrintRequestFunction _sut;

        private Mock<ILogger> _mockLogger;
        private Mock<IPrintRequestCommand> _mockCommand;
        private Mock<ICollector<CertificatePrintStatusUpdateMessage>> _mockCollector;

        public void Arrange(int certificatesReadyToPrint)
        {
            _mockLogger = new Mock<ILogger>();
            _mockCommand = new Mock<IPrintRequestCommand>();
            _mockCollector = new Mock<ICollector<CertificatePrintStatusUpdateMessage>>();

            var messages = certificatesReadyToPrint > 0
                ? Builder<CertificatePrintStatusUpdateMessage>.
                    CreateListOfSize(certificatesReadyToPrint).Build()
                    as List<CertificatePrintStatusUpdateMessage>
                : new List<CertificatePrintStatusUpdateMessage>();

            _mockCommand
                .Setup(m => m.Execute())
                .ReturnsAsync(messages);

            _sut = new Functions.Print.PrintRequestFunction(_mockCommand.Object);
        }

        [TestCase(10)]
        [TestCase(20)]
        [TestCase(0)]
        public async Task ThenItShouldExecuteCommand(int certificatesReadyToPrint)
        {
            Arrange(certificatesReadyToPrint);

            // Act - TimerSchedule is not used so null allowed
            await _sut.Run(new TimerInfo(default, default, false), _mockCollector.Object, _mockLogger.Object);

            // Assert
            _mockCommand.Verify(p => p.Execute(), Times.Once());
            _mockCollector.Verify(p => p.Add(It.IsAny<CertificatePrintStatusUpdateMessage>()), Times.Exactly(certificatesReadyToPrint));
        }
    }
}
