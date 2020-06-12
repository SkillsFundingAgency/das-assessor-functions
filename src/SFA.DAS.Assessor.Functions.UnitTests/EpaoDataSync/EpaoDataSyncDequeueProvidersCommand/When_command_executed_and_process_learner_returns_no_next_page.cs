using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.EpaoDataSync.EpaoDataSyncDequeueProvidersCommand
{
    public class When_command_executed_and_process_learner_returns_no_next_page
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(99)]
        public async Task Then_storage_queue_receives_no_next_page_message(int pageNumber)
        {
            // Arrange
            Mock<IEpaoDataSyncLearnerService> epaoDataSyncLearnerService = new Mock<IEpaoDataSyncLearnerService>();
            Mock<IStorageQueue> storageQueue = new Mock<IStorageQueue>();

            Domain.EpaoDataSync.EpaoDataSyncDequeueProvidersCommand sut = new Domain.EpaoDataSync.EpaoDataSyncDequeueProvidersCommand(epaoDataSyncLearnerService.Object)
            {
                StorageQueue = storageQueue.Object
            };

            JObject inputQueueMessage = new JObject
            {
                { "Source", "1920" },
                { "Ukprn", "222222" },
                { "LearnerPageNumber", pageNumber }
            };

            epaoDataSyncLearnerService.Setup(p => p.ProcessLearners(It.IsAny<EpaoDataSyncProviderMessage>()))
                .ReturnsAsync((EpaoDataSyncProviderMessage)null);

            // Act
            await sut.Execute(inputQueueMessage.ToString());

            // Assert            
            storageQueue.Verify(p => p.AddMessageAsync(It.IsAny<CloudQueueMessage>()), Times.Never());
        }
    }
}
