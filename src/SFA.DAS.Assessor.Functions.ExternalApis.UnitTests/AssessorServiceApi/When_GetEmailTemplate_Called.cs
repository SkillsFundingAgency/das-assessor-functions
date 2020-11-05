using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class When_GetEmailTemplate_Called : AssessorServiceApiTestBase
    {
        [SetUp]
        public override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public async Task Then_ApiCalled_ToGetEmailTemplate()
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
