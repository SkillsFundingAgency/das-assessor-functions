using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Standards
{
    public class StandardImportFunction
    {
        private readonly IStandardImportCommand _command;
        private readonly ILogger<StandardImportFunction> _logger;

        public StandardImportFunction(IStandardImportCommand command, ILogger<StandardImportFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("StandardImport")]
        public async Task Run([TimerTrigger("%StandardImportTimerSchedule%", RunOnStartup = false)]TimerInfo myTimer)
        {
            await FunctionHelper.Run("StandardImport", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}
