﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class EnqueueApprovalLearnerInfoBatchCommand : IEnqueueApprovalLearnerInfoBatchCommand
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<EnqueueApprovalLearnerInfoBatchCommand> _logger;
        private readonly IAssessorServiceRepository _assessorServiceRepository;
        private readonly IQueueService _queueService;

        public EnqueueApprovalLearnerInfoBatchCommand(IOuterApiClient outerApiClient, ILogger<EnqueueApprovalLearnerInfoBatchCommand> logger, IAssessorServiceRepository assessorServiceRepository, IQueueService queueService)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
            _assessorServiceRepository = assessorServiceRepository;
            _queueService = queueService;
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
                const int batchSize = 100;
                int batchNumber = 0;

                GetAllLearnersResponse learnersBatch = await _outerApiClient.Get<GetAllLearnersResponse>(new GetAllLearnersRequest(extractStartTime, batchNumber, batchSize));
                if (learnersBatch == null)
                {
                    string message = $"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}";
                    _logger.LogWarning(message);
                    throw new NullReferenceException(message);
                }

                if (learnersBatch.Learners == null)
                {
                    _logger.LogInformation($"No Approvals Leaners to process");
                    return;
                }

                _logger.LogInformation($"Queueing Approvals Api Learner Batch of {learnersBatch.TotalNumberOfBatches}");

                var learnerBatchMessages = new List<Task>();

                for (int learnerBatchNumber = 1; learnerBatchNumber <= learnersBatch.TotalNumberOfBatches; learnerBatchNumber++)
                {
                    var message = new ProcessApprovalBatchLearnersCommand(learnerBatchNumber);
                    learnerBatchMessages.Add(_queueService.EnqueueMessageAsync(QueueNames.StartUpdateLearnersInfo, message));
                }

                await Task.WhenAll(learnerBatchMessages);

                _logger.LogInformation($"Completed  Approvals Api Learners Batch {learnersBatch.TotalNumberOfBatches} Queue");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ExecuteEnqueueApprovalLearnersBatch failed");
                throw;
            }
        }
    }
}