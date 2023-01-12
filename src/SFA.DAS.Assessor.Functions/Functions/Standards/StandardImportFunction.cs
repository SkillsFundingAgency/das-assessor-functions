using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardImportFunction
    {
        private readonly IStandardImportCommand _command;

        public StandardImportFunction(IStandardImportCommand command)
        {
            _command = command;
        }

        [FunctionName("StandardImport")]
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardImportOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"StandardImport has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"StandardImport has started");
                }

                await _command.Execute();

                log.LogInformation("StandardImport has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "StandardImport has failed");
            }
        }
    }
}
