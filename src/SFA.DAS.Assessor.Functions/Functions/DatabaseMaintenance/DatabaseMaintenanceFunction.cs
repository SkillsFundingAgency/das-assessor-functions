using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.DatabaseMaintenance
{
    public class DatabaseMaintenanceFunction
    {
        private readonly IDatabaseMaintenanceCommand _command;

        public DatabaseMaintenanceFunction(IDatabaseMaintenanceCommand command)
        {
            _command = command;
        }

        [FunctionName("DatabaseMaintenance")]
        public async Task Run([TimerTrigger("%FunctionsOptions:DatabaseMaintenanceOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"DatabaseMaintenance has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"DatabaseMaintenance has started");
                }

                await _command.Execute();

                log.LogInformation("DatabaseMaintenance has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "DatabaseMaintenance has failed");
            }
        }
    }
}
