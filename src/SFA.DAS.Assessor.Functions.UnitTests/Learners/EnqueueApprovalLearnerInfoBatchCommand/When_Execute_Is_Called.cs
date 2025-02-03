using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;

namespace SFA.DAS.Assessor.Functions.UnitTests.Learners.EnqueueApprovalLearnerInfoBatchCommand
{
    public class When_Execute_Is_Called
    {
        [Test]
        public async Task ThenStopProcessingWhenNoLearnersWithoutEmployerInfoIsFoundAsync()
        {
            var ulns = new Dictionary<string, long>();

            var testFixture = new TestFixture();

            await testFixture
                .Setup().WithLearnersEmployerInfoUln(ulns).Execute();

            testFixture.VerifyNoCallToApprovalApi();
        }

        [Test]
        public async Task ThenEnqueueLearnersBatches()
        {
            var ulns = new Dictionary<string, long> { { "100", 100 }, { "200", 200 }, { "300", 300 } };
            var approvalResponse = new GetAllLearnersResponse
            {
                BatchNumber = 1,
                BatchSize = 1,
                Learners = new List<Learner>
                {
                    new Learner
                    {
                        ULN = ulns.ElementAt(0).Key,
                        TrainingCode = "10",
                        EmployerAccountId = 1,
                        EmployerName = "TEST1"
                    },
                    new Learner
                    {
                        ULN =  ulns.ElementAt(1).Key,
                        TrainingCode = "20",
                        EmployerAccountId = 2,
                        EmployerName = "TEST2"
                    },
                },
                TotalNumberOfBatches = 2
            };

            var message1 = new ProcessApprovalBatchLearnersCommand(1);
            var message2 = new ProcessApprovalBatchLearnersCommand(2);

            var testFixture = new TestFixture();
            await testFixture.Setup()
                .WithLearnersEmployerInfoUln(ulns)
                .WithApprovalLearners(approvalResponse)
                .Execute();

            testFixture.VerifyMessageAddedToStorageQueue(message1);
            testFixture.VerifyMessageAddedToStorageQueue(message2);
        }
    }

    internal class TestFixture
    {
        private Domain.Learners.EnqueueApprovalLearnerInfoBatchCommand _sut;
        private Mock<IOuterApiClient> _mockOuterApiClient;
        private ILogger<Domain.Learners.EnqueueApprovalLearnerInfoBatchCommand> _logger;
        private Mock<IAssessorServiceRepository> _mockAssessorServiceRepository;
        public Mock<IQueueService> QueueService;

        public TestFixture Setup()
        {
            _mockOuterApiClient = new Mock<IOuterApiClient>();

            _mockOuterApiClient
                .Setup(x => x.Get<GetAllLearnersResponse>(It.IsAny<GetAllLearnersRequest>()))
                .ReturnsAsync(new GetAllLearnersResponse());

            _logger = Mock.Of<ILogger<Domain.Learners.EnqueueApprovalLearnerInfoBatchCommand>>();

            _mockAssessorServiceRepository = new Mock<IAssessorServiceRepository>();
            _mockAssessorServiceRepository
                .Setup(x => x.GetLearnersWithoutEmployerInfo())
                .ReturnsAsync(new Dictionary<string, long>());

            QueueService = new Mock<IQueueService>();

            _sut = new Domain.Learners.EnqueueApprovalLearnerInfoBatchCommand(_mockOuterApiClient.Object, _logger, _mockAssessorServiceRepository.Object, QueueService.Object);

            return this;
        }

        public async Task Execute()
        {
            await _sut.Execute();
        }

        public TestFixture WithApprovalLearners(GetAllLearnersResponse approvalLearnersResponse)
        {
            _mockOuterApiClient
                .Setup(x => x.Get<GetAllLearnersResponse>(It.IsAny<GetAllLearnersRequest>()))
                .ReturnsAsync(approvalLearnersResponse);

            return this;
        }

        public TestFixture WithLearnersEmployerInfoUln(Dictionary<string, long> ulns)
        {
            _mockAssessorServiceRepository
                .Setup(x => x.GetLearnersWithoutEmployerInfo())
                .ReturnsAsync(ulns);

            return this;
        }

        public void VerifyMessageAddedToStorageQueue(ProcessApprovalBatchLearnersCommand message)
        {
            QueueService.Verify(p => p.EnqueueMessageAsync(QueueNames.StartUpdateLearnersInfo, It.Is<ProcessApprovalBatchLearnersCommand>(m => m.BatchNumber == message.BatchNumber)));
        }

        public void VerifyNoCallToApprovalApi()
        {
            _mockOuterApiClient.Verify(x => x.Get<GetAllLearnersResponse>(It.IsAny<GetAllLearnersRequest>()), Times.Never);
        }

        public void VerifyNoMessageAddedToStorageQueue()
        {
            QueueService.Verify(p => p.EnqueueMessageAsync(QueueNames.StartUpdateLearnersInfo, It.IsAny<ProcessApprovalBatchLearnersCommand>()), Times.Never);
        }
    }
}