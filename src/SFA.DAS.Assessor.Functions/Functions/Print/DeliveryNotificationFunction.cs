﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class DeliveryNotificationFunction
    {
        private readonly IDeliveryNotificationCommand _command;

        public DeliveryNotificationFunction(IDeliveryNotificationCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificateDeliveryNotification")]
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:DeliveryNotificationOptions:Schedule%", RunOnStartup = true)]TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("CertificateDeliveryNotification has started later than scheduled");
                }

                log.LogInformation($"CertificateDeliveryNotification started");

                await _command.Execute(storageQueue);

                log.LogInformation("CertificateDeliveryNotification finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificateDeliveryNotification failed");
            }
        }
    }
}
