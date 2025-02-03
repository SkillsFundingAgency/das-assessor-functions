using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;
using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs
{
    public class RefreshIlrsEnqueueProvidersCommand : IRefreshIlrsEnqueueProvidersCommand
    {
        private readonly IRefreshIlrsAccessorSettingService _refreshIlrsAccessorSettingService;
        private readonly IRefreshIlrsProviderService _refreshIlrsProviderService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IQueueService _queueService;
        private readonly ILogger<RefreshIlrsEnqueueProvidersCommand> _logger;

        public RefreshIlrsEnqueueProvidersCommand(
            IRefreshIlrsAccessorSettingService refreshIlrsAccessorSettingService,
            IRefreshIlrsProviderService refreshIlrsProviderService, 
            IDateTimeHelper dateTimeHelper,
            IQueueService queueService,
            ILogger<RefreshIlrsEnqueueProvidersCommand> logger)
        {
            _refreshIlrsAccessorSettingService = refreshIlrsAccessorSettingService;
            _refreshIlrsProviderService = refreshIlrsProviderService;
            _dateTimeHelper = dateTimeHelper;
            _queueService = queueService;
            _logger = logger;
        }

        public async Task Execute()
        {
            var previousRunDateTime = await _refreshIlrsAccessorSettingService.GetLastRunDateTime();
            var nextRunDateTime = _dateTimeHelper.DateTimeNow;
            
            var output = await _refreshIlrsProviderService.ProcessProviders(previousRunDateTime, nextRunDateTime);
            if (output != null && output.Count > 0)
            {
                foreach (var message in output)
                {
                    await _queueService.EnqueueMessageAsync(QueueNames.RefreshIlrs, message);
                }

                // the last run datetime will only be updated when providers have been queued, this
                // allows for transient downtime on the DC API without missing any provider updates
                await _refreshIlrsAccessorSettingService.SetLastRunDateTime(nextRunDateTime);
            }

            _logger.LogInformation($"Queued {(output?.Count ?? 0)} providers updates between {previousRunDateTime} and {nextRunDateTime}.");
        }
    }
}
