using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class When_UpdateBatchLogReadyToPrintAddCertifictes_Called : AssessorServiceApiTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_ApiCalled_ToUpdateBatchLogReadyToPrintAddCertifictes()
        {
            // Arrange
            var batchNumber = 222;
            var certificatesAdded = 20;
            
            var request = new UpdateBatchLogReadyToPrintAddCertificatesRequest()
            {
                MaxCertificatesToBeAdded = 50
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/v1/batches/{batchNumber}/update-ready-to-print-add-certificates" && r.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(certificatesAdded), Encoding.UTF8, "text/json") })
                .Verifiable();

            // Act
            var result = await _sut.UpdateBatchLogReadyToPrintAddCertifictes(batchNumber, request);

            // Assert
            result.Should().Be(certificatesAdded);
        }
    }
}
