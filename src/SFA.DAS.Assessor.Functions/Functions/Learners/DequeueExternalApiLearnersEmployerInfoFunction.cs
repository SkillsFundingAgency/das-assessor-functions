using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class DequeueExternalApiLearnersEmployerInfoFunction
    {
        private readonly IDequeueLearnerInfoCommand _command;

        public DequeueExternalApiLearnersEmployerInfoFunction(IDequeueLearnerInfoCommand command)
        {
            _command = command;
        }

        [Function("DequeueExternalApiLearnersEmployerInfoFunction")]
        public async Task Run([QueueTrigger(QueueNames.UpdateLearnersInfo)] string message, ILogger log)
        {
            try
            {
                log.LogDebug($"DequeueExternalApiLearnersEmployerInfoFunction has started for {message}");

                await _command.Execute(message);

                log.LogDebug($"DequeueExternalApiLearnersEmployerInfoFunction has finished for {message}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"DequeueExternalApiLearnersEmployerInfoFunction has failed for {message}");
                throw;
            }
        }
    }
}