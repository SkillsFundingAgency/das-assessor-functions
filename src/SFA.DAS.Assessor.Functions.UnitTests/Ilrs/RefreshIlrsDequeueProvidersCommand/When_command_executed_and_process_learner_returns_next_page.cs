using Microsoft.Azure.Functions.Worker;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.RefreshIlrsDequeueProvidersCommand
{
    public class When_command_executed_and_process_learner_returns_next_page
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(99)]
        public async Task Then_storage_queue_receives_next_page_message(int pageNumber)
        {
            // Arrange
            Mock<IRefreshIlrsLearnerService> refreshIlrsLearnerService = new Mock<IRefreshIlrsLearnerService>();
            Mock<IQueueService> queueServiceMock = new Mock<IQueueService>();

            Domain.Ilrs.RefreshIlrsDequeueProvidersCommand sut = new Domain.Ilrs.RefreshIlrsDequeueProvidersCommand(refreshIlrsLearnerService.Object, queueServiceMock.Object);

            RefreshIlrsProviderMessage inputQueueMessage = new RefreshIlrsProviderMessage
            {
                Source = "1920",
                Ukprn = 222222,
                LearnerPageNumber= pageNumber,
            };

            RefreshIlrsProviderMessage outputQueueMessage = new RefreshIlrsProviderMessage
            {
                Source = "1920",
                Ukprn = 222222,
                LearnerPageNumber = pageNumber + 1,
            };

            refreshIlrsLearnerService.Setup(p => p.ProcessLearners(It.IsAny<RefreshIlrsProviderMessage>()))
                .ReturnsAsync((RefreshIlrsProviderMessage message) => new RefreshIlrsProviderMessage
                {
                    Source = message.Source,
                    Ukprn = message.Ukprn,
                    LearnerPageNumber = message.LearnerPageNumber + 1
                });

            // Act
            await sut.Execute(JsonConvert.SerializeObject(inputQueueMessage));

            // Assert
            queueServiceMock.Verify(p => p.EnqueueMessageAsync(QueueNames.RefreshIlrs, It.Is<RefreshIlrsProviderMessage>(m => m.Equals(outputQueueMessage))));
        }
    }
}
