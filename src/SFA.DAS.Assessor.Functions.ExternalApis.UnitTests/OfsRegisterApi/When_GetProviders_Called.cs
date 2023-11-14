using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Types;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.OfsRegisterApi
{
    public class When_GetProviders_Called : OfsRegisterApiTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_ApiCalled_ToGetProviders()
        {
            // Arrange
            var response = new List<OfsProvider>
            {
                new OfsProvider { Ukprn = "12345678", RegistrationStatus = "Registered", HighestLevelOfDegreeAwardingPowers = "Not applicable" },
                new OfsProvider { Ukprn = "23456781", RegistrationStatus = "Registgered", HighestLevelOfDegreeAwardingPowers = "Taught" }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/provider" && r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "text/json") })
                .Verifiable();

            // Act
            var result = await _sut.GetProviders();

            // Assert
            result.Should().Equals(response);
        }
    }
}
