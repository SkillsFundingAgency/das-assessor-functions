using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class DeliveryNotificationFunction
    {
        private readonly IDeliveryNotificationCommand _command;
        private readonly ILogger<DeliveryNotificationFunction> _logger;

        public DeliveryNotificationFunction(IDeliveryNotificationCommand command, ILogger<DeliveryNotificationFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("CertificateDeliveryNotification")]
        [QueueOutput(QueueNames.CertificatePrintStatusUpdate)]
        public async Task<List<CertificatePrintStatusUpdateMessage>> Run(
            [TimerTrigger("%CertificateDeliveryNotificationTimerSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation("CertificateDeliveryNotification has started" + (myTimer.IsPastDue ? " later than scheduled" : string.Empty));

                var printStatusUpdateMessages = await _command.Execute();

                _logger.LogInformation("CertificateDeliveryNotification has finished");

                return printStatusUpdateMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CertificateDeliveryNotification has failed");
                return new List<CertificatePrintStatusUpdateMessage>();
            }
        }
    }
}
