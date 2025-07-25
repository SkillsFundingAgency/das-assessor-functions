﻿using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Assessor.Functions.Functions
{
    public class FunctionHelper
    {
        public async static Task Run(string name, Func<Task> func, TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"{name} has started later than the expected time of {myTimer.ScheduleStatus.Next}");
                }
                else
                {
                    log.LogInformation($"{name} has started");
                }

                await func();

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
