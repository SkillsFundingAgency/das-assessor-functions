using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class BlobStorageSamplesFunction
    {
        private readonly IBlobStorageSamplesCommand _command;

        public BlobStorageSamplesFunction(IBlobStorageSamplesCommand command)
        {
            _command = command;
        }

        [FunctionName("BlobStorageSamples")]
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:BlobStorageSamplesOptions:Schedule%", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("BlobStorageSamples timer trigger has started later than scheduled");
                }

                log.LogInformation($"BlobStorageSamples started");

                await _command.Execute();

                log.LogInformation("BlobStorageSamples finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "BlobStorageSamples failed");
            }
        }
    }
}
