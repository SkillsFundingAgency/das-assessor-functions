using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class EnqueueLearnerInfoCommand : IEnqueueLearnerInfoCommand
    {
        public ICollector<string> StorageQueue { get; set; }

        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<EnqueueLearnerInfoCommand> _logger;
        private readonly IAssessorServiceRepository _assessorServiceRepository;

        public EnqueueLearnerInfoCommand(IOuterApiClient outerApiClient,
            ILogger<EnqueueLearnerInfoCommand> logger,
            IAssessorServiceRepository assessorServiceRepository)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
            _assessorServiceRepository = assessorServiceRepository;
        }

        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("EnqueueLearnerInfoCommand started");

                var learnersToProcessUln = await _assessorServiceRepository.GetLearnersWithoutEmployerInfo();

                if (learnersToProcessUln == null || !learnersToProcessUln.Any())
                {
                    _logger.LogInformation($"There is no learner to process EnqueueLearnerInfoCommand completed");
                    return;
                }

                _logger.LogInformation($"Learners to process {learnersToProcessUln.Count}");

                //1. Get all Learners From Approvals in batches
                DateTime? extractStartTime = new DateTime(2000, 1, 1);
                const int batchSize = 1000;
                int batchNumber = 0;
                int count = 0;
                GetAllLearnersResponse learnersBatch = null;

                do
                {
                    batchNumber++;
                    try
                    {
                        learnersBatch = await _outerApiClient.Get<GetAllLearnersResponse>(new GetAllLearnersRequest(extractStartTime, batchNumber, batchSize));
                        if (learnersBatch?.Learners == null)
                        {
                            _logger.LogWarning($"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}");
                        }

                        _logger.LogInformation($"Approvals batch import loop. Starting batch {batchNumber} of {learnersBatch.TotalNumberOfBatches}");

                        // 2. Check if the learner needs to be processed if yes enqueue
                        foreach (var learner in learnersBatch.Learners)
                        {
                            long uln = 0;
                            if (!long.TryParse(learner.ULN, out uln))
                                continue;

                            int trainingCode = 0;
                            int.TryParse(learner.TrainingCode, out trainingCode);

                            if (learnersToProcessUln.Contains(uln))
                            {
                                var message = new UpdateLearnersInfoMessage(learner.EmployerAccountId, learner.EmployerName, uln, trainingCode);
                                StorageQueue.Add(JsonConvert.SerializeObject(message));
                            }
                        }

                        count += learnersBatch.Learners.Count;
                        _logger.LogInformation($"Approvals batch import loop. Batch Completed {batchNumber} of {learnersBatch.TotalNumberOfBatches}. Total Inserted: {count}");

                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}");

                    }

                } while (batchNumber < learnersBatch.TotalNumberOfBatches);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnqueueLearnerInfoCommand failed");
                throw;
            }
        }


    }
}
