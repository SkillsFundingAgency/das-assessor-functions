using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Print
{
    public class PrintFunctionFlow
    {
        private readonly IPrintProcessCommand _command;

        public PrintFunctionFlow(IPrintProcessCommand command)
        {
            _command = command;
        }

        [FunctionName("CertificatePrintFunction")]
        public async Task Run([TimerTrigger("%CertificatePrintFunctionSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao Importer PrintFunctionFlow timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao Importer PrintFunctionFlow started");

                // DEBUG DEBUG DEBUG CON-2396 - FORCE A TIMEOUT WHICH IS 30 MINUTES BY DEFAULT
                await Task.Delay(TimeSpan.FromMinutes(35));
                // DEBUG DEBUG DEBUG CON-2396 - FORCE A TIMEOUT WHICH IS 30 MINUTES BY DEFAULT

                //await _command.Execute();

                log.LogInformation("Epao Importer PrintFunctionFlow function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao Importer PrintFunctionFlow function failed");
            }
        }
    }
}
