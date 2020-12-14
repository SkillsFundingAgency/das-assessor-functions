using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using System.Net.Http;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.AssessorServiceApi
{
    public class AssessorServiceApiTestBase
    {
        protected AssessorServiceApiClient _sut;

        protected Mock<HttpMessageHandler> _mockHttpMessageHandler;
        protected Mock<IOptions<AssessorApiAuthentication>> _mockOptions;
        protected Mock<ILogger<AssessorServiceApiClient>> _mockLogger;

        public virtual void Arrange()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<AssessorServiceApiClient>>();

            _mockOptions = new Mock<IOptions<AssessorApiAuthentication>>();
            _mockOptions.Setup(m => m.Value).Returns(new AssessorApiAuthentication { ApiBaseAddress = "http://localhost:8080" });

            _sut = new AssessorServiceApiClient(new HttpClient(_mockHttpMessageHandler.Object), _mockOptions.Object, _mockLogger.Object);
        }
    }
}
