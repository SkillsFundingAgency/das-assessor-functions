using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

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
                _command.StorageQueue = updateLearnersQueue;
                await _command.Execute(message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"EnqueueExternalApiLearnersEmployerInfo has failed.");
                throw;
            }
        }
    }
}