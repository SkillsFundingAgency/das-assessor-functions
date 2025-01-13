using Microsoft.Azure.Functions.Worker;
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

        [Function("BlobSasTokenGenerator")]
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:BlobSasTokenGeneratorOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("BlobSasTokenGenerator has started later than scheduled");
                }

                log.LogInformation($"BlobSasTokenGenerator started");

                await _command.Execute();

                log.LogInformation("BlobSasTokenGenerator finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "BlobSasTokenGenerator failed");
            }
        }
    }
}
