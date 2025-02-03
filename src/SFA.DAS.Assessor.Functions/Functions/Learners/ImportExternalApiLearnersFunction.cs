using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class ImportExternalApiLearnersFunction
    {
        private readonly IImportLearnersCommand _command;
        private readonly ILogger<ImportExternalApiLearnersFunction> _logger;

        public ImportExternalApiLearnersFunction(IImportLearnersCommand command, ILogger<ImportExternalApiLearnersFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("ImportLearners")]
        public async Task Run([TimerTrigger("%ImportLearnersTimerSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            await FunctionHelper.Run("ImportLearners", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}
