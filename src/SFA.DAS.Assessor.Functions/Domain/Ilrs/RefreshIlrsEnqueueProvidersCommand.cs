using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs
{
    public class RefreshIlrsEnqueueProvidersCommand : IRefreshIlrsEnqueueProvidersCommand
    {
        private readonly IRefreshIlrsAccessorSettingService _refreshIlrsAccessorSettingService;
        private readonly IRefreshIlrsProviderService _refreshIlrsProviderService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILogger<RefreshIlrsEnqueueProvidersCommand> _logger;

        public RefreshIlrsEnqueueProvidersCommand(
            IRefreshIlrsAccessorSettingService refreshIlrsAccessorSettingService,
            IRefreshIlrsProviderService refreshIlrsProviderService, 
            IDateTimeHelper dateTimeHelper,
            ILogger<RefreshIlrsEnqueueProvidersCommand> logger)
        {
            _refreshIlrsAccessorSettingService = refreshIlrsAccessorSettingService;
            _refreshIlrsProviderService = refreshIlrsProviderService;
            _dateTimeHelper = dateTimeHelper;
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
                    StorageQueue.Add(JsonConvert.SerializeObject(message));
                }

                // the last run datetime will only be updated when providers have been queued, this
                // allows for transient downtime on the DC API without missing any provider updates
                await _refreshIlrsAccessorSettingService.SetLastRunDateTime(nextRunDateTime);
            }

            _logger.LogInformation($"Queued {(output?.Count ?? 0)} providers updates between {previousRunDateTime} and {nextRunDateTime}.");
        }

        public ICollector<string> StorageQueue { get; set; }
    }
}
