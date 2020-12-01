using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintStatusUpdateFunction

    {
        private readonly IPrintStatusUpdateCommand _command;

        public PrintStatusUpdateFunction(IPrintStatusUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintStatusUpdate")]
        public async Task Run([QueueTrigger(QueueNames.CertificatePrintStatusUpdate)] string message,
            [Queue(QueueNames.CertificatePrintStatusUpdateErrors)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                log.LogDebug($"CertificatePrintStatusUpdate has started for {message}");

                var validationErrorMessages = await _command.Execute(message);
                validationErrorMessages?.ForEach(p => storageQueue.Add(p));

                if (validationErrorMessages.Count > 0)
                {
                    log.LogInformation($"CertificatePrintStatusUpdate has completed for {message} with {validationErrorMessages.Count} error(s)");
                }
                else
                {
                    log.LogDebug($"CertificatePrintStatusUpdate has completed for {message}");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"CertificatePrintStatusUpdate has failed for {message}");
                throw;
            }
        }
    }
}
