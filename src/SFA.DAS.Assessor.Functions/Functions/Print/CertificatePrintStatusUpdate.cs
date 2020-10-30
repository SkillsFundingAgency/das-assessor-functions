using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class CertificatePrintStatusUpdate
    {
        private readonly ICertificatePrintStatusUpdateCommand _command;

        public CertificatePrintStatusUpdate(ICertificatePrintStatusUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificateCertificatePrintStatusUpdate")]
        public async Task Run(
            [QueueTrigger(QueueNames.CertificatePrintStatusUpdate)] string message,
            ILogger log)
        {
            try
            {
                log.LogDebug($"CertificatePrintStatusUpdate has started for {message}");

                await _command.Execute(message);

                log.LogDebug($"CertificatePrintStatusUpdate has completed for {message}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"CertificatePrintStatusUpdate has failed for {message}");
                throw;
            }
        }
    }
}
