using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class BlobSasTokenGeneratorFunction
    {
        private readonly IBlobSasTokenGeneratorCommand _command;

        public BlobSasTokenGeneratorFunction(IBlobSasTokenGeneratorCommand command)
        {
            _command = command;
        }

        [FunctionName("BlobSasTokenGeneratorFunction")]
        public async Task Run([TimerTrigger("%FunctionsSettings:BlobSasTokenGeneratorFunction:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("BlobSasTokenGeneratorFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"BlobSasTokenGeneratorFunction started");

                await _command.Execute();

                log.LogInformation("BlobSasTokenGeneratorFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "BlobSasTokenGeneratorFunction function failed");
            }
        }
    }
}
