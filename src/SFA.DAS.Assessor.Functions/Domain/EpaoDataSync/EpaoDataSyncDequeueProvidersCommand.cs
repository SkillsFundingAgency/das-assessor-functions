using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync
{
    public class EpaoDataSyncDequeueProvidersCommand : IEpaoDataSyncDequeueProvidersCommand
    {
        private readonly IEpaoDataSyncLearnerService _epaoDataSyncLearnerService;

        public EpaoDataSyncDequeueProvidersCommand(IEpaoDataSyncLearnerService epaoDataSyncLearnerService)
        {
            _epaoDataSyncLearnerService = epaoDataSyncLearnerService;
        }

        public async Task Execute(string message)
        {
            var providerMessage = JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(message);
            var nextPageProviderMessage = await _epaoDataSyncLearnerService.ProcessLearners(providerMessage);
            if (nextPageProviderMessage != null)
            {
                await StorageQueue.AddMessageAsync(
                    new CloudQueueMessage(JsonConvert.SerializeObject(nextPageProviderMessage)));
            }
        }

        public IStorageQueue StorageQueue { get; set; }
    }
}
