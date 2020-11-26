﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintRequestFunction
    {
        private readonly IPrintRequestCommand _command;

        public PrintRequestFunction(IPrintRequestCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintRequest")]
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintRequestOptions:Schedule%", RunOnStartup = true)] TimerInfo myTimer,
            [Queue(QueueNames.CertificatePrintStatusUpdate)] ICollector<string> storageQueue,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("CertificatePrintRequest has started later than scheduled");
                }

                log.LogInformation($"CertificatePrintRequest started");

                await _command.Execute(storageQueue);

                log.LogInformation("CertificatePrintRequest finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificatePrintRequest failed");
            }
        }
    }
}
