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
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;

namespace SFA.DAS.Assessor.Functions.UnitTests.Learners.EnqueueLearnerInfoCommand
{
    public class When_Execute_Is_Called
    {
        [Test]
        public void ThenStopProcessingWhenNoLearnersWithoutEmployerInfoIsFound()
        {
            var testFixture = new TestFixture();
            testFixture.Setup();
            testFixture.VerifyNoCallToApprovalApi();
        }

        [Test]
        public async Task ThenStopProcessingWhenNoLearnersWithoutEmployerInfoIsNull()
        {
            var batchLearnersCommand = new ProcessApprovalBatchLearnersCommand(1);
            string message = JsonConvert.SerializeObject(batchLearnersCommand);

            var testFixture = new TestFixture();
            await testFixture.Setup()
                .WithLearnersEmployerInfoUln(null)
                .WithApprovalLearners(null)
                .Execute(message);
            testFixture.VerifyNoCallToApprovalApi();
        }

        [Test]
        public async Task ThenStopProcessingWhenApprovalLearnersIsNull()
        {
            var batchLearnersCommand = new ProcessApprovalBatchLearnersCommand(1);
            string message = JsonConvert.SerializeObject(batchLearnersCommand);

            var testFixture = new TestFixture();
            var ulns = new Dictionary<string, long> { { "100", 100 }, { "200", 200 }, { "300", 300 } };

            await testFixture.Setup()
                .WithLearnersEmployerInfoUln(ulns)
                .WithApprovalLearners(null)
                .Execute(message);
            testFixture.VerifyNoMessageAddedToStorageQueue();
        }

        [Test]
        public async Task ThenEnqueueLearnersWithoutEmployerInfo()
        {
            var batchLearnersCommand = new ProcessApprovalBatchLearnersCommand(1);
            string message = JsonConvert.SerializeObject(batchLearnersCommand);
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
                TotalNumberOfBatches = 1
            };

            var message1 = new UpdateLearnersInfoMessage(1, "TEST1", 100, 10);
            var message2 = new UpdateLearnersInfoMessage(2, "TEST2", 200, 20);

            var testFixture = new TestFixture();
            await testFixture.Setup()
                .WithLearnersEmployerInfoUln(ulns)
                .WithApprovalLearners(approvalResponse)
                .Execute(message);

            testFixture.VerifyMessageAddedToStorageQueue(message1);
            testFixture.VerifyMessageAddedToStorageQueue(message2);
        }
    }

    internal class TestFixture
    {
        private Domain.Learners.EnqueueLearnerInfoCommand _sut;
        private Mock<IOuterApiClient> _mockOuterApiClient;
        private ILogger<Domain.Learners.EnqueueLearnerInfoCommand> _logger;
        private Mock<IAssessorServiceRepository> _mockAssessorServiceRepository;
        public Mock<IQueueService> _mockQueueService = new Mock<IQueueService>();

        public TestFixture Setup()
        {
            _mockOuterApiClient = new Mock<IOuterApiClient>();
            _mockOuterApiClient
                .Setup(x => x.Get<GetAllLearnersResponse>(It.IsAny<GetAllLearnersRequest>()))
                .ReturnsAsync(new GetAllLearnersResponse());

            _logger = Mock.Of<ILogger<Domain.Learners.EnqueueLearnerInfoCommand>>();

            _mockAssessorServiceRepository = new Mock<IAssessorServiceRepository>();
            _mockAssessorServiceRepository
                .Setup(x => x.GetLearnersWithoutEmployerInfo())
                .ReturnsAsync(new Dictionary<string, long>());

            _sut = new Domain.Learners.EnqueueLearnerInfoCommand(
                _mockOuterApiClient.Object,
                _logger,
                _mockAssessorServiceRepository.Object,
                _mockQueueService.Object);

            return this;
        }

        public async Task Execute(string message)
        {
            await _sut.Execute(message);
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

        public void VerifyMessageAddedToStorageQueue(UpdateLearnersInfoMessage message)
        {
            _mockQueueService.Verify(p => p.EnqueueMessageAsync(QueueNames.UpdateLearnersInfo, It.Is<UpdateLearnersInfoMessage>(m => MessageEquals(m, message))));
        }

        public void VerifyNoCallToApprovalApi()
        {
            _mockOuterApiClient.Verify(x => x.Get<GetAllLearnersResponse>(It.IsAny<GetAllLearnersRequest>()), Times.Never);
        }

        public void VerifyNoMessageAddedToStorageQueue()
        {
            _mockQueueService.Verify(p => p.EnqueueMessageAsync(QueueNames.UpdateLearnersInfo, It.IsAny<UpdateLearnersInfoMessage>()), Times.Never);
        }

        private bool MessageEquals(UpdateLearnersInfoMessage firstMessage, UpdateLearnersInfoMessage secondMessage)
        {
            return firstMessage.Equals(secondMessage);
        }
    }
}