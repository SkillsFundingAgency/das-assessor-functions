using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsEnqueueProvidersFunctionFlow
    {
        private readonly IRefreshIlrsEnqueueProvidersCommand _command;
        private readonly RefreshIlrsSettings _settings;

        public RefreshIlrsEnqueueProvidersFunctionFlow(IRefreshIlrsEnqueueProvidersCommand command, IOptions<RefreshIlrsSettings> options)
        {
            _command = command;
            _settings = options?.Value;
        }

        [FunctionName("RefreshIlrsEnqueueProviders")]
        public async Task Run([TimerTrigger("%FunctionsSettings:RefreshIlrs:EnqueueProvidersSchedule%", RunOnStartup = false)]TimerInfo myTimer,
            [Queue(QueueNames.RefreshIlrs)] ICollector<string> refreshIlrsQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao RefreshIlrsEnqueueProviders has started later than scheduled");

                    if (myTimer.ScheduleStatus.Last < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(_settings.EnqueueProvidersMaxPastDueMinutes)))
                    {
                        log.LogError($"Epao RefreshIlrsEnqueueProviders has exceeded {_settings.EnqueueProvidersMaxPastDueMinutes} minutes past due time and will next run at {myTimer.Schedule.GetNextOccurrence(DateTime.UtcNow)}");
                        return;
                    }
                }
                else
                {
                    log.LogInformation("Epao RefreshIlrsEnqueueProviders has started");
                }

                _command.StorageQueue = refreshIlrsQueue;
                await _command.Execute();

                log.LogInformation("Epao RefreshIlrsEnqueueProviders has completed");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Epao RefreshIlrsEnqueueProviders has failed");
            }
        }
    }
}
