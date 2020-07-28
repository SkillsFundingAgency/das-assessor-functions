using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class WhenSystemChangesStatusesToPrinted
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

        [TestCase(1, 1)]
        [TestCase(99, 1)]
        [TestCase(100, 1)]
        [TestCase(101, 2)]
        [TestCase(199, 2)]
        [TestCase(200, 2)]
        [TestCase(201, 3)]
        [TestCase(299, 3)]
        [TestCase(300, 3)]
        [TestCase(301, 4)]
        [TestCase(399, 4)]
        [TestCase(400, 4)]
        [TestCase(401, 5)]
        [TestCase(499, 5)]
        [TestCase(500, 5)]
        public async Task ThenItShouldUpdateCertificatesInChunksOf100(int batchSize, int chunksSent)
        {
            // Arrange
            var batchNumber = 1;
            var certificates = Builder<CertificateToBePrintedSummary>.CreateListOfSize(batchSize).Build();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )                
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent("")})
                .Verifiable();

            // Act
            await _sut.ChangeStatusToPrinted(batchNumber, certificates);

            // Assert
            _mockHttpMessageHandler
                .Protected()
                .Verify("SendAsync",
                Times.Exactly(chunksSent),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Put && r.RequestUri.AbsolutePath == $"/api/v1/certificates/{batchNumber}"),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
