using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Ilrs
{
    public class RefreshIlrsEnqueueProvidersFunctionFlow
    {
        private readonly IRefreshIlrsEnqueueProvidersCommand _command;

        public RefreshIlrsEnqueueProvidersFunctionFlow(IRefreshIlrsEnqueueProvidersCommand command)
        {
            _command = command;
        }

        [FunctionName("RefreshIlrsEnqueueProviders")]
        public async Task Run([TimerTrigger("%RefreshIlrsEnqueueProvidersFunctionFlowSchedule%", RunOnStartup = true)]TimerInfo myTimer,
            [Queue(QueueNames.RefreshIlrs, Connection = "StorageAccountConnectionString")]CloudQueue refreshIlrsQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("RefreshIlrsEnqueueProviders has started later than scheduled");
                }
                else
                {
                    log.LogInformation("RefreshIlrsEnqueueProviders has started");
                }

                _command.StorageQueue = new StorageQueue(refreshIlrsQueue);
                await _command.Execute();

                log.LogInformation("RefreshIlrsEnqueueProviders has completed");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "RefreshIlrsEnqueueProviders has failed");
            }
        }
    }
}
