using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class BlobStorageSamplesFunction
    {
        private readonly IBlobStorageSamplesCommand _command;
        private readonly ILogger<BlobStorageSamplesFunction> _logger;

        public BlobStorageSamplesFunction(IBlobStorageSamplesCommand command, ILogger<BlobStorageSamplesFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("BlobStorageSamples")]
        public async Task Run([TimerTrigger("%BlobStorageSamplesTimerSchedule%", RunOnStartup = true)]TimerInfo myTimer)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    _logger.LogInformation("BlobStorageSamples timer trigger has started later than scheduled");
                }

                _logger.LogInformation($"BlobStorageSamples started");

                await _command.Execute();

                _logger.LogInformation("BlobStorageSamples finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BlobStorageSamples failed");
            }
        }
    }
}
