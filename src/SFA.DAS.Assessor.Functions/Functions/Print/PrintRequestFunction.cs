using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

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
        public async Task Run([TimerTrigger("%FunctionsOptions:PrintCertificatesOptions:PrintRequestOptions:Schedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("CertificatePrintRequest has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"CertificatePrintRequest has started");
                }

                await _command.Execute();

                log.LogInformation("CertificatePrintRequest has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "CertificatePrintRequest has failed");
            }
        }
    }
}
