using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
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
                if (string.IsNullOrEmpty(message))
                {
                    _logger.LogWarning("Received a null or empty message.");
                    throw new ArgumentException("Message cannot be null or empty");
                }

                await _command.Execute(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnqueueExternalApiLearnersEmployerInfo has failed.");
                throw;
            }
        }
    }
}