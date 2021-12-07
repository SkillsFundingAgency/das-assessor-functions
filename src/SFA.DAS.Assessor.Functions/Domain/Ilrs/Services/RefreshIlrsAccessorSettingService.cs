using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Services
{
    public class RefreshIlrsAccessorSettingService : IRefreshIlrsAccessorSettingService
    {
        private readonly RefreshIlrsOptions _refreshIlrsOptions;
        private readonly IAssessorServiceApiClient _assessorApiClient;
        private readonly ILogger<RefreshIlrsAccessorSettingService> _logger;

        private const string RefreshIlrsLastRunDate = "RefreshIlrsLastRunDate";

        public RefreshIlrsAccessorSettingService(IOptions<RefreshIlrsOptions> options, IAssessorServiceApiClient assessorServiceApiClient, 
            ILogger<RefreshIlrsAccessorSettingService> logger)
        {
            _refreshIlrsOptions = options?.Value;
            _assessorApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public async Task<DateTime> GetLastRunDateTime()
        {
            try
            {
                var lastRunDateTimeSetting = await _assessorApiClient.GetAssessorSetting(RefreshIlrsLastRunDate);
                if (lastRunDateTimeSetting != null)
                {
                    if (DateTime.TryParse(lastRunDateTimeSetting, out DateTime lastRunDateTime))
                        return lastRunDateTime;
                }
            }
            catch
            {
                _logger.LogInformation($"There is no {RefreshIlrsLastRunDate}, using default last run date {_refreshIlrsOptions.ProviderInitialRunDate}");
            }

            return _refreshIlrsOptions.ProviderInitialRunDate;
        }

        public async Task SetLastRunDateTime(DateTime lastRunDateTime)
        {
            await _assessorApiClient.SetAssessorSetting(RefreshIlrsLastRunDate, lastRunDateTime.ToString("o"));
        }
    }
}
