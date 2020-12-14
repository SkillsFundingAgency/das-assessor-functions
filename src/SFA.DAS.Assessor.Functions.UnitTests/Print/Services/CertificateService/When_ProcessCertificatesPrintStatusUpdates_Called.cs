using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Services.CertificateService
{
    public class When_ProcessCertificatesPrintStatusUpdates_Called
    {
        private Mock<IAssessorServiceApiClient> _mockAssessorServiceApiClient;
        protected Mock<ILogger<Domain.Print.Services.CertificateService>> _mockLogger;
        
        [SetUp]
        public void Arrange()
        {
            _mockAssessorServiceApiClient = new Mock<IAssessorServiceApiClient>();
            _mockLogger = new Mock<ILogger<Domain.Print.Services.CertificateService>>();
        }

        [Test]
        public async Task Then_AssessorApiCalled_ToGetBatch()
        {
            // Arrange
            var sut = new Domain.Print.Services.CertificateService(_mockAssessorServiceApiClient.Object, _mockLogger.Object);
            var certificatePrintStatusUpdate = new CertificatePrintStatusUpdate
            {
                BatchNumber = 1,
                CertificateReference = "00010111",
                ReasonForChange = "",
                Status = CertificateStatus.Delivered,
                StatusAt = DateTime.UtcNow
            };
            
            // Act
            await sut.ProcessCertificatesPrintStatusUpdate(certificatePrintStatusUpdate);

            // Assert
            _mockAssessorServiceApiClient.Verify(v => v.UpdateCertificatesPrintStatus(
                It.Is<CertificatesPrintStatusUpdateRequest>(c => JsonConvert.SerializeObject(c).Equals(JsonConvert.SerializeObject(certificatePrintStatusUpdate)))),
                Times.Once);
        }
    }
}
