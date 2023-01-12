using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsEnqueueProvidersFunction
    {
        private readonly IRefreshIlrsEnqueueProvidersCommand _command;
        private readonly RefreshIlrsOptions _settings;

        public RefreshIlrsEnqueueProvidersFunction(IRefreshIlrsEnqueueProvidersCommand command, IOptions<RefreshIlrsOptions> options)
        {
            _command = command;
            _settings = options?.Value;
        }

        [FunctionName("RefreshIlrsEnqueueProviders")]
        public async Task Run([TimerTrigger("%FunctionsOptions:RefreshIlrsOptions:EnqueueProvidersSchedule%", RunOnStartup = false)] TimerInfo myTimer,
            [Queue(QueueNames.RefreshIlrs)] ICollector<string> refreshIlrsQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("RefreshIlrsEnqueueProviders has started later than scheduled");

                    if (myTimer.ScheduleStatus.Last < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(_settings.EnqueueProvidersMaxPastDueMinutes)))
                    {
                        log.LogError($"RefreshIlrsEnqueueProviders has exceeded {_settings.EnqueueProvidersMaxPastDueMinutes} minutes past due time and will next run at {myTimer.Schedule.GetNextOccurrence(DateTime.UtcNow)}");
                        return;
                    }
                }
                else
                {
                    log.LogInformation("RefreshIlrsEnqueueProviders has started");
                }

                _command.StorageQueue = refreshIlrsQueue;
                await _command.Execute();

                log.LogInformation("RefreshIlrsEnqueueProviders has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "RefreshIlrsEnqueueProviders has failed");
            }
        }
    }
}
