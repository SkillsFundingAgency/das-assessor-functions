using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.PrintStatusUpdateCommand
{
    public class When_Execute_Called
    {
        private Domain.Print.PrintStatusUpdateCommand _sut;

        private Mock<ILogger<Domain.Print.PrintStatusUpdateCommand>> _mockLogger;
        private Mock<ICertificateService> _mockCertificateService;

        private CertificatePrintStatusUpdateMessage _certificatePrintStatusUpdateMessage;

        [SetUp]
        public void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.PrintStatusUpdateCommand>>();
            _mockCertificateService = new Mock<ICertificateService>();

            _certificatePrintStatusUpdateMessage = new CertificatePrintStatusUpdateMessage
            {
                BatchNumber = 1,
                CertificateReference = "00010111",
                ReasonForChange = "",
                Status = CertificateStatus.Delivered,
                StatusAt = DateTime.UtcNow
            };

            _mockCertificateService
                .Setup(m => m.ProcessCertificatesPrintStatusUpdate(
                    It.Is<CertificatePrintStatusUpdateMessage>(
                        p => CertificatePrintStatusUpdateMessageEquals(p, _certificatePrintStatusUpdateMessage))))
                .ReturnsAsync(new ValidationResponse(new ValidationErrorDetail()));

            _sut = new Domain.Print.PrintStatusUpdateCommand(
                _mockLogger.Object,
                _mockCertificateService.Object);
        }

        [Test]
        public async Task ThenItShouldCallCertificateServiceToProcessUpdates()
        {
            // Arrange
            var message = _certificatePrintStatusUpdateMessage;

            // Act
            await _sut.Execute(message);

            // Assert
            _mockCertificateService.Verify(m => m.ProcessCertificatesPrintStatusUpdate(
                It.Is<CertificatePrintStatusUpdateMessage>(
                        p => CertificatePrintStatusUpdateMessageEquals(p, _certificatePrintStatusUpdateMessage))),
                Times.Once);
        }

        private bool CertificatePrintStatusUpdateMessageEquals(CertificatePrintStatusUpdateMessage first, CertificatePrintStatusUpdateMessage second)
        {
            return JsonConvert.SerializeObject(first).Equals(JsonConvert.SerializeObject(second));
        }
    }
}
