using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ApiClient.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class WhenSystemGetsEmailTemplate
    {
        private AssessorServiceApiClient _sut;

        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
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

        [Test]
        public async Task ThenItShouldReturnAnEmailTemplate()
        {
            // Arrange
            var emailTemplate = Builder<EmailTemplateSummary>.CreateNew().Build();
            var templateName = EMailTemplateNames.PrintAssessorCoverLetters;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.AbsolutePath == $"/api/v1/emailTemplates/{templateName}" && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(emailTemplate), Encoding.UTF8, "text/json") })
                .Verifiable();

            // Act
            var result = await _sut.GetEmailTemplate(templateName);

            // Assert
            result.Should().BeOfType<EmailTemplateSummary>();
            (result as EmailTemplateSummary).TemplateName.Should().Be(emailTemplate.TemplateName);
        }
    }
}
