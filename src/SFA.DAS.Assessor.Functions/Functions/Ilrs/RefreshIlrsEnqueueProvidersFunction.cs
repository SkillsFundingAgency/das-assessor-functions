using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsEnqueueProvidersFunction
    {
        private readonly IRefreshIlrsEnqueueProvidersCommand _command;
        private readonly RefreshIlrsOptions _settings;
        private readonly ILogger<RefreshIlrsEnqueueProvidersFunction> _logger;

        public RefreshIlrsEnqueueProvidersFunction(
            IRefreshIlrsEnqueueProvidersCommand command, 
            IOptions<RefreshIlrsOptions> options,
            ILogger<RefreshIlrsEnqueueProvidersFunction> logger)
        {
            _command = command;
            _settings = options?.Value;
            _logger = logger;
        }

        [Function("RefreshIlrsEnqueueProviders")]
        public async Task Run(
            [TimerTrigger("%FunctionsOptions:RefreshIlrsOptions:EnqueueProvidersSchedule%", RunOnStartup = false)]TimerInfo myTimer)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    _logger.LogInformation("RefreshIlrsEnqueueProviders has started later than scheduled");

                    if (myTimer.ScheduleStatus.Last < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(_settings.EnqueueProvidersMaxPastDueMinutes)))
                    {
                        _logger.LogError($"RefreshIlrsEnqueueProviders has exceeded {_settings.EnqueueProvidersMaxPastDueMinutes} minutes past due time and will next run at {myTimer.ScheduleStatus.Next}");
                        return;
                    }
                }
                else
                {
                    _logger.LogInformation("RefreshIlrsEnqueueProviders has started");
                }

                await _command.Execute();

                _logger.LogInformation("RefreshIlrsEnqueueProviders has finished");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "RefreshIlrsEnqueueProviders has failed");
            }
        }
    }
}
