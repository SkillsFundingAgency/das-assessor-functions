using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Learners
{
    public class EnqueueExternalApiLearnersEmployerInfoFunction
    {
        private readonly IEnqueueLearnerInfoCommand _command;

        public EnqueueExternalApiLearnersEmployerInfoFunction(IEnqueueLearnerInfoCommand command)
        {
            _command = command;
        }

        [FunctionName("EnqueueExternalApiLearnersEmployerInfoFunction")]
        public async Task Run([QueueTrigger(QueueNames.StartUpdateLearnersInfo)] string message,
            [Queue(QueueNames.UpdateLearnersInfo)] IAsyncCollector<UpdateLearnersInfoMessage> updateLearnersQueue,
            ILogger log)
        {
            try
            {
                log.LogDebug($"EnqueueExternalApiLearnersEmployerInfo has started.");

                _command.StorageQueue = updateLearnersQueue;
                await _command.Execute(message);

                log.LogDebug($"EnqueueExternalApiLearnersEmployerInfo has finished.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"EnqueueExternalApiLearnersEmployerInfo has failed.");
                throw;
            }
        }
    }
}