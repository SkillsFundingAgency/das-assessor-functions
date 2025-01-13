using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
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

        [Function("StartLearnersEmployerInfoUpdateFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                _logger.LogDebug($"StartLearnersEmployerInfoUpdateFunction has started.");

                await _command.Execute();

                _logger.LogDebug($"StartLearnersEmployerInfoUpdateFunction has finished.");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"StartLearnersEmployerInfoUpdateFunction has failed.");
                throw;
            }

            return new OkObjectResult("Start Learners EmployerInfo Update Function finished");
        }
    }
}