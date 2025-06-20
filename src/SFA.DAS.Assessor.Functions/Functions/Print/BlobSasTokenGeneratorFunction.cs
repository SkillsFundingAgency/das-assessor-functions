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
        private readonly ILogger<BlobSasTokenGeneratorFunction> _logger;

        public BlobSasTokenGeneratorFunction(IBlobSasTokenGeneratorCommand command, ILogger<BlobSasTokenGeneratorFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("BlobSasTokenGenerator")]
        public async Task Run([TimerTrigger("%BlobSasTokenGeneratorTimerSchedule%", RunOnStartup = false)]TimerInfo myTimer)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    _logger.LogInformation("BlobSasTokenGenerator has started later than scheduled");
                }

                _logger.LogInformation($"BlobSasTokenGenerator started");

                await _command.Execute();

                _logger.LogInformation("BlobSasTokenGenerator finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BlobSasTokenGenerator failed");
            }
        }
    }
}
