﻿using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using System;
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

        [FunctionName("ImportLearners")]
        public async Task Run([TimerTrigger("%FunctionsOptions:ImportLearnersOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogDebug($"ImportLearners has started.");

                await _command.Execute();

                log.LogDebug($"ImportLearners has finished.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"ImportLearners has failed.");
                throw;
            }
        }
    }
}