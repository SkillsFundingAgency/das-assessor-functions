using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Apar
{
    public class OfsImportFunction
    {
        private readonly IOfsImportCommand _command;

        public OfsImportFunction(IOfsImportCommand command)
        {
            _command = command;
        }

        [FunctionName("OfsImport")]
        public async Task Run([TimerTrigger("%FunctionsOptions:OfsImportOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"OfsImport has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"OfsImport has started");
                }

                await _command.Execute();

                log.LogInformation("OfsImport has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "OfsImport has failed");
            }
        }
    }
}
