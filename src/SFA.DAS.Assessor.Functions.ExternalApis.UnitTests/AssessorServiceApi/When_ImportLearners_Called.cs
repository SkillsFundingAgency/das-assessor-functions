using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class When_ImportLearners_Called : AssessorServiceApiTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_ApiCalled_ToImportLearners()
        {
            // Arrange

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/approvals/update-approvals" && r.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK })
                .Verifiable();

            // Act
            await _sut.ImportLearners();

            // Assert
            _mockHttpMessageHandler.Verify();
        }
    }
}
