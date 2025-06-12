using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;
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
        public async Task Run([TimerTrigger("%FunctionsOptions:DatabaseMaintenanceOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("DatabaseMaintenance", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
