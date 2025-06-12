using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

namespace SFA.DAS.Assessor.Functions.UnitTests.Learners.DequeueLearnerInfoCommand
{
    public class When_Execute_Is_Called
    {
        [Test]
        public async Task ThenProcessLearnerMessage()
        {
            var learnersInfoMessage = new UpdateLearnersInfoMessage(1, "TEST1", 100, 10);
            string message = JsonConvert.SerializeObject(learnersInfoMessage);

            var testFixture = new TestFixture();
            await testFixture.Setup().Execute(message);

            testFixture.VerifyProcessLearnerMessage();
        }
    }
    class TestFixture
    {
        private Domain.Learners.DequeueLearnerInfoCommand _sut;
        private ILogger<Domain.Learners.DequeueLearnerInfoCommand> _logger;
        private Mock<IAssessorServiceRepository> _mockAssessorServiceRepository;
        public Mock<ICollector<string>> StorageQueue = new Mock<ICollector<string>>();

        public TestFixture Setup()
        {
            _logger = Mock.Of<ILogger<Domain.Learners.DequeueLearnerInfoCommand>>();

            _mockAssessorServiceRepository = new Mock<IAssessorServiceRepository>();
            _mockAssessorServiceRepository
                .Setup(x => x.UpdateLearnerInfo(It.IsAny<(long uln, int standardCode, long employerAccountId, string employerName)>()));

            _sut = new Domain.Learners.DequeueLearnerInfoCommand(_logger, _mockAssessorServiceRepository.Object);
            return this;
        }

        public async Task Execute(string message)
        {
            await _sut.Execute(message);
        }

        public void VerifyProcessLearnerMessage()
        {
            _mockAssessorServiceRepository
                .Verify(x => x.UpdateLearnerInfo(It.IsAny<(long uln, int standardCode, long employerAccountId, string employerName)>()),
                    Times.Once);
        }

    }
}
