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
    public class When_GetCreateBatchLog_Called : AssessorServiceApiTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_ApiCalled_ToCreateBatchLog()
        {
            // Arrange
            var utcDate = DateTime.UtcNow;

            var batchResponse = Builder<BatchLogResponse>.CreateNew().Build();
            batchResponse.ScheduledDate = utcDate;

            var request = new CreateBatchLogRequest()
            {
                ScheduledDate = utcDate
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/v1/batches/create" && r.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(batchResponse), Encoding.UTF8, "text/json") })
                .Verifiable();

            // Act
            var result = await _sut.CreateBatchLog(request);

            // Assert
            result.Should().Equals(batchResponse);
        }
    }
}
