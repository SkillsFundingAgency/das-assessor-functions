using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class ImportExternalApiLearnersFunction
    {
        private readonly IImportLearnersCommand _command;

        public ImportExternalApiLearnersFunction(IImportLearnersCommand command)
        {
            _command = command;
        }

        [Function("ImportLearners")]
        public async Task Run([TimerTrigger("%FunctionsOptions:ImportLearnersOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("ImportLearners", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
