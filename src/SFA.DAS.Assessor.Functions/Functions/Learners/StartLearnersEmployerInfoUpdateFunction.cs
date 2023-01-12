using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class StartLearnersEmployerInfoUpdateFunction
    {
        private readonly IEnqueueApprovalLearnerInfoBatchCommand _command;

        public StartLearnersEmployerInfoUpdateFunction(IEnqueueApprovalLearnerInfoBatchCommand command)
        {
            _command = command;
        }

        [FunctionName("StartLearnersEmployerInfoUpdateFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue(QueueNames.StartUpdateLearnersInfo)] IAsyncCollector<ProcessApprovalBatchLearnersCommand> startUpdatingLearnersQueue,
            ILogger log)
        {
            try
            {
                log.LogDebug($"StartLearnersEmployerInfoUpdateFunction has started.");

                _command.StorageQueue = startUpdatingLearnersQueue;
                await _command.Execute();

                log.LogDebug($"StartLearnersEmployerInfoUpdateFunction has finished.");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"StartLearnersEmployerInfoUpdateFunction has failed.");
                throw;
            }

            return new OkObjectResult("Start Learners EmployerInfo Update Function finished");
        }
    }
}