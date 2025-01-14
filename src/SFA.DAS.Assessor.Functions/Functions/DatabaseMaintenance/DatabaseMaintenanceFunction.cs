using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.DatabaseMaintenance
{
    public class DatabaseMaintenanceFunction
    {
        private readonly IDatabaseMaintenanceCommand _command;
        private readonly ILogger<DatabaseMaintenanceFunction> _logger;

        public DatabaseMaintenanceFunction(IDatabaseMaintenanceCommand command, ILogger<DatabaseMaintenanceFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("DatabaseMaintenance")]
        public async Task Run([TimerTrigger("%DatabaseMaintenanceTimerSchedule%", RunOnStartup = false)]TimerInfo myTimer)
        {
            await FunctionHelper.Run("DatabaseMaintenance", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}
