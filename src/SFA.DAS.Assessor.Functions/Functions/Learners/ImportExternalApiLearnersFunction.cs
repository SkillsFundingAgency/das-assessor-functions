using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class ImportExternalApiLearnersFunction : TimerTriggerFunction
    {
        private readonly IImportLearnersCommand _command;

        public ImportExternalApiLearnersFunction(IImportLearnersCommand command)
        {
            _command = command;
        }

        [FunctionName("ImportLearners")]
        public async Task Run([TimerTrigger("%FunctionsOptions:ImportLearnersOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await base.Run("ImportLearners", _command, myTimer, log);
        }
    }
}
