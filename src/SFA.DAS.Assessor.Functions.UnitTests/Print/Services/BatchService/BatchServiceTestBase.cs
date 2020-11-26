using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Services.BatchService
{
    public class BatchServiceTestBase
    {
        protected Domain.Print.Services.BatchService _sut;

        protected Mock<ILogger<Domain.Print.Services.BatchService>> _mockLogger;
        protected Mock<IAssessorServiceApiClient> _mockAssessorServiceApiClient;

        protected ValidationResponse _validResponse = new ValidationResponse();
        protected ValidationResponse _invalidResponse = new ValidationResponse(new ValidationErrorDetail());

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
