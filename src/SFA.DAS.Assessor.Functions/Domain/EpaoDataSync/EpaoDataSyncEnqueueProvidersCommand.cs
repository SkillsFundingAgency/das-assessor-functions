using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync
{
    public class EpaoDataSyncEnqueueProvidersCommand : IEpaoDataSyncEnqueueProvidersCommand
    {
        private readonly IEpaoDataSyncProviderService _epaoDataSyncProviderService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public EpaoDataSyncEnqueueProvidersCommand(
            IEpaoDataSyncProviderService epaoDataSyncProviderService, 
            IDateTimeHelper dateTimeHelper)
        {
            _epaoDataSyncProviderService = epaoDataSyncProviderService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task Execute()
        {
            var output = await _epaoDataSyncProviderService.ProcessProviders();
            if (output != null)
            {
                foreach (var message in output)
                {
                    await StorageQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
                }

                await _epaoDataSyncProviderService.SetLastRunDateTime(_dateTimeHelper.DateTimeNow);
            }
        }

        public IStorageQueue StorageQueue { get; set; }
    }
}
