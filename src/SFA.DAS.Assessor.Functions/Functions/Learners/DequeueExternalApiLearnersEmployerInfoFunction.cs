using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class DequeueExternalApiLearnersEmployerInfoFunction
    {
        private readonly IDequeueLearnerInfoCommand _command;
        private readonly ILogger<DequeueExternalApiLearnersEmployerInfoFunction> _logger;

        public DequeueExternalApiLearnersEmployerInfoFunction(IDequeueLearnerInfoCommand command, ILogger<DequeueExternalApiLearnersEmployerInfoFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("DequeueExternalApiLearnersEmployerInfoFunction")]
        public async Task Run([QueueTrigger(QueueNames.UpdateLearnersInfo)] string message)
        {
            try
            {
                await _command.Execute(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DequeueExternalApiLearnersEmployerInfoFunction has failed for {message}");
                throw;
            }
        }
    }
}