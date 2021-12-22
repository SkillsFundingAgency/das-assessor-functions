using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class DequeueLearnerInfoCommand : IDequeueLearnerInfoCommand
    {
        public ICollector<string> StorageQueue { get; set; }

        private readonly ILearnersInfoService _learnersInfoService;
        private readonly ILogger<DequeueLearnerInfoCommand> _logger;

        public DequeueLearnerInfoCommand(ILearnersInfoService learnersInfoService, ILogger<DequeueLearnerInfoCommand> logger)
        {
            _logger = logger;
            _learnersInfoService = learnersInfoService;
        }

        public async Task Execute(string message)
        {
            _logger.LogInformation("DequeueLearnerInfoCommand started");

            var learners = JsonConvert.DeserializeObject<List<UpdateLearnersInfoMessage>>(message);

            _logger.LogInformation($"Started processing {learners.Count} learner employer information ");

            var nextLearnerToProcess = await _learnersInfoService.ProcessLearners(learners);

            if (!nextLearnerToProcess.Any())
            {
                _logger.LogInformation("DequeueLearnerInfoCommand Completed no learner to queue");
                return;
            }

            var queueMessage = JsonConvert.SerializeObject(nextLearnerToProcess);
            StorageQueue.Add(queueMessage);

            _logger.LogInformation("DequeueLearnerInfoCommand Completed");
        }

    }
}
