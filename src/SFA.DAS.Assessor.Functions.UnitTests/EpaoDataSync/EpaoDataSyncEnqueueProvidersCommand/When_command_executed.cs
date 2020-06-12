using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.EpaoDataSync.EpaoDataSyncEnqueueProvidersCommand
{
    public class When_command_executed
    {
        Mock<IEpaoDataSyncProviderService> _epaoDataSyncProviderService = new Mock<IEpaoDataSyncProviderService>();
        Mock<IStorageQueue> _storageQueue = new Mock<IStorageQueue>();
        Mock<IDateTimeHelper> _dateTimeHelper = new Mock<IDateTimeHelper>();
        
        Domain.EpaoDataSync.EpaoDataSyncEnqueueProvidersCommand _sut;

        EpaoDataSyncProviderMessage provider1 = new EpaoDataSyncProviderMessage
        {
            Ukprn = 111111,
            Source = "1920",
            LearnerPageNumber = 1
        };

        EpaoDataSyncProviderMessage provider2 = new EpaoDataSyncProviderMessage
        {
            Ukprn = 222222,
            Source = "1920",
            LearnerPageNumber = 1
        };

        EpaoDataSyncProviderMessage provider3 = new EpaoDataSyncProviderMessage
        {
            Ukprn = 3333333,
            Source = "2021",
            LearnerPageNumber = 1
        };

        [SetUp]
        public void Arrange()
        {
            // Arrange
            _sut = new Domain.EpaoDataSync.EpaoDataSyncEnqueueProvidersCommand(
                    _epaoDataSyncProviderService.Object, _dateTimeHelper.Object)
            {
                StorageQueue = _storageQueue.Object
            };
        }
        
        [Test]
        public async Task Then_process_provider_is_called()
        {
            // Arrange
            Arrange();

            _epaoDataSyncProviderService.Setup(p => p.ProcessProviders()).ReturnsAsync(
                new List<EpaoDataSyncProviderMessage>());

            // Act
            await _sut.Execute();

            // Assert            
            _epaoDataSyncProviderService.Verify(p => p.ProcessProviders(), Times.Once());
        }

        [Test]
        public async Task Then_storage_queue_received_provider()
        {
            // Arrange
            Arrange();

            _epaoDataSyncProviderService.Setup(p => p.ProcessProviders()).ReturnsAsync(
                new List<EpaoDataSyncProviderMessage>
                {
                    provider1,
                    provider2,
                    provider3
                });

            // Act
            await _sut.Execute();

            // Assert            
            _storageQueue.Verify(p => p.AddMessageAsync(It.Is<CloudQueueMessage>(m => MessageEquals(m, new CloudQueueMessage(JsonConvert.SerializeObject(provider1))))));
            _storageQueue.Verify(p => p.AddMessageAsync(It.Is<CloudQueueMessage>(m => MessageEquals(m, new CloudQueueMessage(JsonConvert.SerializeObject(provider2))))));
            _storageQueue.Verify(p => p.AddMessageAsync(It.Is<CloudQueueMessage>(m => MessageEquals(m, new CloudQueueMessage(JsonConvert.SerializeObject(provider3))))));
        }

        private bool MessageEquals(CloudQueueMessage first, CloudQueueMessage second)
        {
            var firstMessage = JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(first.AsString);
            var secondMessage = JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(second.AsString);

            return firstMessage.Equals(secondMessage);
        }
    }
}
