using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs
{
    public class RefreshIlrsDequeueProvidersCommand : IRefreshIlrsDequeueProvidersCommand
    {
        private readonly IRefreshIlrsLearnerService _refreshIlrsLearnerService;

        public RefreshIlrsDequeueProvidersCommand(IRefreshIlrsLearnerService refreshIlrsLearnerService)
        {
            _refreshIlrsLearnerService = refreshIlrsLearnerService;
        }

        public async Task Execute(string message)
        {
            var providerMessage = JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(message);
            var nextPageProviderMessage = await _refreshIlrsLearnerService.ProcessLearners(providerMessage);
            if (nextPageProviderMessage != null)
            {
                StorageQueue.Add(JsonConvert.SerializeObject(nextPageProviderMessage));
            }
        }

        public ICollector<string> StorageQueue { get; set; }
    }
}
