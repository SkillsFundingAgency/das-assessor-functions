using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions
{
    public class TimerTriggerFunction
    {
        public async Task Run(string name, ICommand command, TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"{name} has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"{name} has started");
                }

                await command.Execute();

                log.LogInformation($"{name} has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"{name} has failed");
                throw;
            }
        }
    }
}
