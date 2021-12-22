using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class EnqueueLearnerInfoCommand : IEnqueueLearnerInfoCommand
    {
        public ICollector<string> StorageQueue { get; set; }

        private readonly ILearnersInfoService _learnersInfoService;
        private readonly ILogger<EnqueueLearnerInfoCommand> _logger;

        public EnqueueLearnerInfoCommand(ILearnersInfoService learnersInfoService, ILogger<EnqueueLearnerInfoCommand> logger)
        {
            _logger = logger;
            _learnersInfoService = learnersInfoService;
        }
        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("EnqueueLearnerInfoCommand started");

                var learnersToProcessUln = await _learnersInfoService.GetLearnersToUpdate();

                if (learnersToProcessUln == null || !learnersToProcessUln.Any())
                {
                    _logger.LogInformation($"There is no learner to process EnqueueLearnerInfoCommand completed");
                    return;
                }
                
                var message = JsonConvert.SerializeObject(learnersToProcessUln);
                StorageQueue.Add(message);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnqueueLearnerInfoCommand failed");
                throw;
            }
        }



    }
}
