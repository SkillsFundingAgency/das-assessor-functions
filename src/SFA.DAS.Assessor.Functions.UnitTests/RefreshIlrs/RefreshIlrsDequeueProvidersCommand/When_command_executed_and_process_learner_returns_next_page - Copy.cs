using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.RefreshIlrs.RefreshIlrsDequeueProvidersCommand
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
            Mock<IStorageQueue> storageQueue = new Mock<IStorageQueue>();

            Domain.Ilrs.RefreshIlrsDequeueProvidersCommand sut = new Domain.Ilrs.RefreshIlrsDequeueProvidersCommand(refreshIlrsLearnerService.Object)
            {
                StorageQueue = storageQueue.Object
            };

            JObject inputQueueMessage = new JObject
            {
                { "Source", "1920" },
                { "Ukprn", "222222" },
                { "LearnerPageNumber", pageNumber }
            };

            JObject outputQueueMessage = new JObject
            {
                { "Source", "1920" },
                { "Ukprn", "222222" },
                { "LearnerPageNumber", pageNumber + 1 }
            };

            refreshIlrsLearnerService.Setup(p => p.ProcessLearners(It.IsAny<RefreshIlrsProviderMessage>()))
                .ReturnsAsync((RefreshIlrsProviderMessage message) => new RefreshIlrsProviderMessage 
                { 
                    Source = message.Source, 
                    Ukprn = message.Ukprn, 
                    LearnerPageNumber = message.LearnerPageNumber + 1 
                });

            // Act
            await sut.Execute(inputQueueMessage.ToString());

            // Assert            
            storageQueue.Verify(p => p.AddMessageAsync(It.Is<CloudQueueMessage>(m => MessageEquals(m, new CloudQueueMessage(outputQueueMessage.ToString())))));
        }

        private bool MessageEquals(CloudQueueMessage first, CloudQueueMessage second)
        {
            var firstMessage = JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(first.AsString);
            var secondMessage = JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(second.AsString);

            return firstMessage.Equals(secondMessage);
        }
    }
}
