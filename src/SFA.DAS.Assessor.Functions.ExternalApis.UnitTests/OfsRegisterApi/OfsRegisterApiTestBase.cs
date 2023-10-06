using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Authentication;
using System.Net.Http;

namespace SFA.DAS.Assessor.Functions.ExternalApis.UnitTests.OfsRegisterApi
{
    public class OfsRegisterApiTestBase
    {
        protected OfsRegisterApiClient _sut;

        protected Mock<HttpMessageHandler> _mockHttpMessageHandler;
        protected Mock<IOptions<OfsRegisterApiAuthentication>> _mockOptions;
        protected Mock<ILogger<OfsRegisterApiClient>> _mockLogger;

        public virtual void Arrange()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<OfsRegisterApiClient>>();

            _mockOptions = new Mock<IOptions<OfsRegisterApiAuthentication>>();
            _mockOptions.Setup(m => m.Value).Returns(new OfsRegisterApiAuthentication { ApiBaseAddress = "http://localhost:8080" });

            _sut = new OfsRegisterApiClient(new HttpClient(_mockHttpMessageHandler.Object), _mockOptions.Object, _mockLogger.Object);
        }
    }
}
