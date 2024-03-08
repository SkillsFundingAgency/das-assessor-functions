using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
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
        public async Task Run([TimerTrigger("%FunctionsOptions:StandardImportOptions:Schedule%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("StandardImport", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
