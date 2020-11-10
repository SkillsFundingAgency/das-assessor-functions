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

        [FunctionName("CertificateDeliveryNotificationFunction")]
        public async Task Run([TimerTrigger("%FunctionsSettings:CertificateDeliveryNotificationFunction:Schedule%", RunOnStartup = true)]TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao CertificateDeliveryNotificationFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao CertificateDeliveryNotificationFunction started");

                await _command.Execute(storageQueue);

                log.LogInformation("Epao CertificateDeliveryNotificationFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao CertificateDeliveryNotificationFunction function failed");
            }
        }
    }
}
