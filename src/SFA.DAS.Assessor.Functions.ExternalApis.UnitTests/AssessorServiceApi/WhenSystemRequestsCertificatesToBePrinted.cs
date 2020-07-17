using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class WhenSystemRequestsCertificatesToBePrinted
    {
        private AssessorServiceApiClient _sut;

        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<IAssessorServiceTokenService> _mockAssessorServiceTokenService;
        private Mock<IOptions<AssessorApiAuthentication>> _mockOptions;
        private Mock<ILogger<AssessorServiceApiClient>> _mockLogger;

        [SetUp]
        public void Arrange()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockAssessorServiceTokenService = new Mock<IAssessorServiceTokenService>();
            _mockOptions = new Mock<IOptions<AssessorApiAuthentication>>();
            _mockLogger = new Mock<ILogger<AssessorServiceApiClient>>();

            _mockOptions.Setup(m => m.Value).Returns(new AssessorApiAuthentication { ApiBaseAddress = "http://localhost:8080" });

            _sut = new AssessorServiceApiClient(new HttpClient(_mockHttpMessageHandler.Object), _mockAssessorServiceTokenService.Object, _mockOptions.Object, _mockLogger.Object);
        }

        [Test]
        public async Task ThenItShouldReturnAnEmailTemplate()
        {
            // Arrange
            var certificatesToPrintedResponse = new CertificatesToBePrintedResponse()
            {
                Certificates = Builder<CertificateToBePrintedSummary>
                    .CreateListOfSize(10)
                    .Build() as List<CertificateToBePrintedSummary>
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/v1/certificates/tobeprinted" && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(certificatesToPrintedResponse), Encoding.UTF8, "text/json") })
                .Verifiable();

            // Act
            var result = await _sut.GetCertificatesToBePrinted();

            // Assert
            result.Certificates.Should().HaveCount(10);
        }
    }
}
