using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class StartLearnersEmployerInfoUpdateFunction
    {
        private readonly IEnqueueApprovalLearnerInfoBatchCommand _command;
        private readonly ILogger<StartLearnersEmployerInfoUpdateFunction> _logger;

        public StartLearnersEmployerInfoUpdateFunction(IEnqueueApprovalLearnerInfoBatchCommand command, ILogger<StartLearnersEmployerInfoUpdateFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        /*
        This function has been commented as it's currently not in use.We may need it sometime soon for the digital certificates.
        [Function("StartLearnersEmployerInfoUpdateFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                _logger.LogInformation($"StartLearnersEmployerInfoUpdateFunction has started.");

                await _command.Execute();

                _logger.LogInformation($"StartLearnersEmployerInfoUpdateFunction has finished.");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"StartLearnersEmployerInfoUpdateFunction has failed.");
                throw;
            }

            return new OkObjectResult("Start Learners EmployerInfo Update Function finished");
            }
        */
    }
}