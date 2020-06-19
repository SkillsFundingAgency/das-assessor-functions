using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs
{
    public class RefreshIlrsEnqueueProvidersCommand : IRefreshIlrsEnqueueProvidersCommand
    {
        private readonly IRefreshIlrsProviderService _refreshIlrsProviderService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public RefreshIlrsEnqueueProvidersCommand(
            IRefreshIlrsProviderService refreshIlrsProviderService, 
            IDateTimeHelper dateTimeHelper)
        {
            _refreshIlrsProviderService = refreshIlrsProviderService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task Execute()
        {
            var output = await _refreshIlrsProviderService.ProcessProviders();
            if (output != null)
            {
                foreach (var message in output)
                {
                    await StorageQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
                }

                await _refreshIlrsProviderService.SetLastRunDateTime(_dateTimeHelper.DateTimeNow);
            }
        }

        public IStorageQueue StorageQueue { get; set; }
    }
}
