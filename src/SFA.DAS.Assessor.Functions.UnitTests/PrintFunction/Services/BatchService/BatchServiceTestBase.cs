using Microsoft.Extensions.Logging;
using Microsoft.OData.UriParser;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;

namespace SFA.DAS.Assessor.Functions.UnitTests.PrintFunction.Services.BatchService
{
    public class BatchServiceTestBase
    {
        protected Domain.Print.Services.BatchService _sut;

        protected Mock<ILogger<Domain.Print.Services.BatchService>> _mockLogger;
        protected Mock<IAssessorServiceApiClient> _mockAssessorServiceApiClient;

        protected int _batchNumber = 111;

        public virtual void Arrange()
        {
            _mockLogger = new Mock<ILogger<Domain.Print.Services.BatchService>>();
            _mockAssessorServiceApiClient = new Mock<IAssessorServiceApiClient>();
            
            _sut = new Domain.Print.Services.BatchService(
                _mockAssessorServiceApiClient.Object,
                _mockLogger.Object
                );
        }
    }
}
