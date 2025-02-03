using FizzWare.NBuilder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.UnitTests.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintRequestFunction
{
    public class When_Run_Called
    {
        private Functions.Print.PrintRequestFunction _sut;
        
        private Mock<ILogger<Functions.Print.PrintRequestFunction>> _mockLogger;
        private Mock<IPrintRequestCommand> _mockCommand;

        public void Arrange(int certificatesReadyToPrint)
        {
            _mockLogger = new Mock<ILogger<Functions.Print.PrintRequestFunction>>();
            _mockCommand = new Mock<IPrintRequestCommand>();

            var messages = certificatesReadyToPrint > 0
                ? Builder<CertificatePrintStatusUpdateMessage>.
                    CreateListOfSize(certificatesReadyToPrint).Build()
                    as List<CertificatePrintStatusUpdateMessage>
                : new List<CertificatePrintStatusUpdateMessage>();
            
            _mockCommand
                .Setup(m => m.Execute())
                .ReturnsAsync(messages);

            _sut = new Functions.Print.PrintRequestFunction(_mockCommand.Object, _mockLogger.Object);
        }

        [TestCase(10)]
        [TestCase(20)]
        [TestCase(0)]
        public async Task ThenItShouldExecuteCommand(int certificatesReadyToPrint)
        {
            Arrange(certificatesReadyToPrint);
            
            // Act
            TimerInfo timerInfo = TimerInfoFactory.Create();    
            await _sut.Run(timerInfo);

            // Assert
            _mockCommand.Verify(p => p.Execute(), Times.Once());
        }
    }
}
