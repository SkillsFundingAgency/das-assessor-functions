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
    public class EnqueueApprovalLearnerInfoBatchCommand : IEnqueueApprovalLearnerInfoBatchCommand
    {
        public ICollector<string> StorageQueue { get; set; }

        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<EnqueueApprovalLearnerInfoBatchCommand> _logger;
        private readonly IAssessorServiceRepository _assessorServiceRepository;

        public EnqueueApprovalLearnerInfoBatchCommand(IOuterApiClient outerApiClient, ILogger<EnqueueApprovalLearnerInfoBatchCommand> logger, IAssessorServiceRepository assessorServiceRepository)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
            _assessorServiceRepository = assessorServiceRepository;
        }

        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("ExecuteEnqueueApprovalLearnersBatch started");

                var learnersToProcessUln = await _assessorServiceRepository.GetLearnersWithoutEmployerInfo();

                if (learnersToProcessUln == null || !learnersToProcessUln.Any())
                {
                    _logger.LogInformation($"There is no learner to process ExecuteEnqueueApprovalLearnersBatch completed");
                    return;
                }
                _logger.LogInformation($"Learners to process {learnersToProcessUln.Count}");

                //1. Get all Learners From Approvals in batches
                DateTime? extractStartTime = new DateTime(2000, 1, 1);
                const int batchSize = 1000;
                int batchNumber = 0;

                GetAllLearnersResponse learnersBatch = await _outerApiClient.Get<GetAllLearnersResponse>(new GetAllLearnersRequest(extractStartTime, batchNumber, batchSize));
                if (learnersBatch?.Learners == null)
                {
                    _logger.LogWarning($"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}");
                    return;
                }

                for (int learnerBatchNumber = 1; learnerBatchNumber <= learnersBatch.TotalNumberOfBatches; learnerBatchNumber++)
                {
                    var message = new ProcessApprovalBatchLearnersCommand(learnerBatchNumber);
                    StorageQueue.Add(JsonConvert.SerializeObject(message));
                }

                _logger.LogInformation($"Approvals batch import loop. Starting batch {batchNumber} of {learnersBatch.TotalNumberOfBatches}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ExecuteEnqueueApprovalLearnersBatch failed");
                throw;
            }
        }
    }
}