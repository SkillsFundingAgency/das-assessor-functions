using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Ofs
{
    public class OfsImportFunction
    {
        private readonly IOfsImportCommand _command;
        private readonly ILogger<OfsImportFunction> _logger;

        public OfsImportFunction(IOfsImportCommand command, ILogger<OfsImportFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("OfsImport")]
        public async Task Run([TimerTrigger("%FunctionsOptions:OfsImportOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    _logger.LogInformation($"OfsImport has started later than the expected time of {myTimer.ScheduleStatus.Next}");
                }
                else
                {
                    _logger.LogInformation($"OfsImport has started");
                }

                await _command.Execute();

                _logger.LogInformation("OfsImport has finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OfsImport has failed");
            }
        }
    }
}
