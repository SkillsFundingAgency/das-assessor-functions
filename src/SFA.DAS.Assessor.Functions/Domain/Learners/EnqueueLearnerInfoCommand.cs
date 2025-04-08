using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class EnqueueLearnerInfoCommand : IEnqueueLearnerInfoCommand
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<EnqueueLearnerInfoCommand> _logger;
        private readonly IAssessorServiceRepository _assessorServiceRepository;
        private readonly IQueueService _queueService;

        public EnqueueLearnerInfoCommand(IOuterApiClient outerApiClient,
            ILogger<EnqueueLearnerInfoCommand> logger,
            IAssessorServiceRepository assessorServiceRepository,
            IQueueService queueService)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
            _assessorServiceRepository = assessorServiceRepository;
            _queueService = queueService;
        }

        public async Task Execute(string batchMessage)
        {
            try
            {
                _logger.LogInformation("EnqueueLearnerInfoCommand started");

                _logger.LogInformation($"Batch message received  {batchMessage}");

                try
                {
                    var cmd = JsonConvert.DeserializeObject<ProcessApprovalBatchLearnersCommand>(batchMessage);
                }
                catch (Exception ex)
                { 
                    _logger.LogInformation($"Excepetion deserialising message {ex.Message}");
                    throw new Exception("EnqueueLearnerInfoCommand deserialise error", ex);
                }


                var approvalBatchLearnersCommand = JsonConvert.DeserializeObject<ProcessApprovalBatchLearnersCommand>(batchMessage);

                _logger.LogInformation($"Started processing approval batch  {approvalBatchLearnersCommand.BatchNumber}");

                var learnersToProcessUln = await _assessorServiceRepository.GetLearnersWithoutEmployerInfo();

                if (learnersToProcessUln == null || !learnersToProcessUln.Any())
                {
                    _logger.LogInformation($"There is no learner to process EnqueueLearnerInfoCommand completed");
                    return;
                }
                _logger.LogInformation($"Learners to process {learnersToProcessUln.Count}");

                //1. Get all Learners From Approvals in batches
                DateTime? extractStartTime = new DateTime(2000, 1, 1);
                const int batchSize = 100;
                int batchNumber = approvalBatchLearnersCommand.BatchNumber;
                GetAllLearnersResponse learnersBatch = null;

                try
                {
                    learnersBatch = await _outerApiClient.Get<GetAllLearnersResponse>(new GetAllLearnersRequest(extractStartTime, approvalBatchLearnersCommand.BatchNumber, batchSize));
                    if (learnersBatch == null)
                    {
                        string message = $"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}";
                        _logger.LogWarning(message);
                        return;
                    }

                    if (learnersBatch.Learners == null)
                    {
                        _logger.LogInformation($"No Approvals Leaners to process for batch {batchNumber}");
                        return;
                    }

                    _logger.LogInformation($"Approvals batch import loop. Starting batch {batchNumber} of {learnersBatch.TotalNumberOfBatches}");

                    // 2. Check if the learner needs to be processed if yes enqueue
                    var learnersToProcessUlnMessages = new List<Task>();
                    foreach (var learner in learnersBatch.Learners)
                    {
                        long uln = 0;
                        int trainingCode = 0;

                        int.TryParse(learner.TrainingCode, out trainingCode);

                        if (string.IsNullOrWhiteSpace(learner.ULN))
                        {
                            _logger.LogWarning($"Invalid ULN {learner.ULN}");
                            continue;
                        }

                        if (learnersToProcessUln.TryGetValue(learner.ULN, out uln))
                        {
                            var message = new UpdateLearnersInfoMessage(learner.EmployerAccountId, learner.EmployerName, uln, trainingCode);
                            learnersToProcessUlnMessages.Add(_queueService.EnqueueMessageAsync(QueueNames.UpdateLearnersInfo, message));
                        }
                    }

                    await Task.WhenAll(learnersToProcessUlnMessages);

                    _logger.LogInformation($"Approvals batch import loop. Batch Completed {batchNumber} of {learnersBatch.TotalNumberOfBatches}.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnqueueLearnerInfoCommand failed");
                throw;
            }
        }
    }
}