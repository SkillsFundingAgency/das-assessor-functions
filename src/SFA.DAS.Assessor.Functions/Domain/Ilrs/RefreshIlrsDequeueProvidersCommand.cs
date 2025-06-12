using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs
{
    public class RefreshIlrsDequeueProvidersCommand : IRefreshIlrsDequeueProvidersCommand
    {
        private readonly IRefreshIlrsLearnerService _refreshIlrsLearnerService;
        private readonly IQueueService _queueService;

        public RefreshIlrsDequeueProvidersCommand(IRefreshIlrsLearnerService refreshIlrsLearnerService, IQueueService queueService)
        {
            _refreshIlrsLearnerService = refreshIlrsLearnerService;
            _queueService = queueService;
        }

        public async Task Execute(string message)
        {
            var providerMessage = JsonConvert.DeserializeObject<RefreshIlrsProviderMessage>(message);
            var nextPageProviderMessage = await _refreshIlrsLearnerService.ProcessLearners(providerMessage);
            if (nextPageProviderMessage != null)
            {
                await _queueService.EnqueueMessageAsync(QueueNames.RefreshIlrs, nextPageProviderMessage);
            }
    }
}
}
