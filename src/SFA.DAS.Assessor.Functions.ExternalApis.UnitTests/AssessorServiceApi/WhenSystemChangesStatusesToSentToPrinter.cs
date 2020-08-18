using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class WhenSystemChangesStatusesToSentToPrinter
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
            _mockOptions = new Mock<IOptions<AssessorApiAuthentication>>();
            _mockLogger = new Mock<ILogger<AssessorServiceApiClient>>();

            _mockOptions.Setup(m => m.Value).Returns(new AssessorApiAuthentication { ApiBaseAddress = "http://localhost:8080" });

            _sut = new AssessorServiceApiClient(new HttpClient(_mockHttpMessageHandler.Object), _mockOptions.Object, _mockLogger.Object);
        }

        [TestCase(1, 1)]
        [TestCase(99, 5)]
        [TestCase(100, 5)]
        [TestCase(101, 6)]
        [TestCase(199, 10)]
        [TestCase(200, 10)]
        [TestCase(201, 11)]
        [TestCase(299, 15)]
        [TestCase(300, 15)]
        [TestCase(301, 16)]
        [TestCase(399, 20)]
        [TestCase(400, 20)]
        [TestCase(401, 21)]
        [TestCase(499, 25)]
        [TestCase(500, 25)]
        public async Task ThenItShouldUpdateCertificatesInChunksOf100(int batchSize, int chunksSent)
        {
            // Arrange
            var batchNumber = 1;
            var certificateResponses = new List<string>();
            var generator = new RandomGenerator();
            for (int i = 0; i < batchSize; i++)
            {
                certificateResponses.Add(generator.Phrase(15));
            }

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
            await _sut.SaveSentToPrinter(batchNumber, certificateResponses);

            // Assert
            _mockHttpMessageHandler
                .Protected()
                .Verify("SendAsync",
                Times.Exactly(chunksSent),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Put && r.RequestUri.AbsolutePath == $"/api/v1/batches/sent-to-printer"),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
