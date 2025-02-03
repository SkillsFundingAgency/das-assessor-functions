using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class EnqueueExternalApiLearnersEmployerInfoFunction
    {
        private readonly IEnqueueLearnerInfoCommand _command;
        private readonly ILogger<EnqueueExternalApiLearnersEmployerInfoFunction> _logger;

        public EnqueueExternalApiLearnersEmployerInfoFunction(IEnqueueLearnerInfoCommand command, ILogger<EnqueueExternalApiLearnersEmployerInfoFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("EnqueueExternalApiLearnersEmployerInfoFunction")]
        public async Task Run([QueueTrigger(QueueNames.StartUpdateLearnersInfo)] string message)
        {
            try
            {
                _logger.LogDebug($"EnqueueExternalApiLearnersEmployerInfo has started.");

                await _command.Execute(message);

                _logger.LogDebug($"EnqueueExternalApiLearnersEmployerInfo has finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnqueueExternalApiLearnersEmployerInfo has failed.");
                throw;
            }
        }
    }
}