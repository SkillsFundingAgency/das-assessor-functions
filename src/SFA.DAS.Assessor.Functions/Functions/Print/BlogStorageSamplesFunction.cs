using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class BlogStorageSamplesFunction
    {
        private readonly IBlogStorageSamplesFunctionCommand _command;

        public BlogStorageSamplesFunction(IBlogStorageSamplesFunctionCommand command)
        {
            _command = command;
        }

        [FunctionName("BlogStorageSamplesFunction")]
        public async Task Run([TimerTrigger("%FunctionsSettings:BlogStorageSamplesFunction:Schedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao BlogStorageSamplesFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao BlogStorageSamplesFunction started");

                await _command.Execute();

                log.LogInformation("Epao BlogStorageSamplesFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao BlogStorageSamplesFunction function failed");
            }
        }
    }
}
