using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class BlobStorageSamplesFunction
    {
        private readonly IBlobStorageSamplesCommand _command;

        public BlobStorageSamplesFunction(IBlobStorageSamplesCommand command)
        {
            _command = command;
        }

        [FunctionName("BlobStorageSamplesFunction")]
        public async Task Run([TimerTrigger("%FunctionsSettings:BlobStorageSamplesFunction:Schedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao BlobStorageSamplesFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao BlobStorageSamplesFunction started");

                await _command.Execute();

                log.LogInformation("Epao BlobStorageSamplesFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao BlobStorageSamplesFunction function failed");
            }
        }
    }
}
