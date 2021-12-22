using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Services
{
    public class LearnersInfoService : ILearnersInfoService
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<LearnersInfoService> _logger;
        private readonly IAssessorServiceRepository _assessorServiceRepository;

        public LearnersInfoService(IOuterApiClient outerApiClient,
            ILogger<LearnersInfoService> logger,
            IAssessorServiceRepository assessorServiceRepository)
        {
            _outerApiClient = outerApiClient;
            _logger = logger;
            _assessorServiceRepository = assessorServiceRepository;
        }

        public async Task<List<UpdateLearnersInfoMessage>> GetLearnersToUpdate()
        {

            var learnersToUpdate = new List<UpdateLearnersInfoMessage>();

            var learnersToProcessUln = await _assessorServiceRepository.GetLearnersWithoutEmployerInfo();

            if (learnersToProcessUln == null || !learnersToProcessUln.Any())
            {
                return new List<UpdateLearnersInfoMessage>();
            }

            _logger.LogInformation($"Learners to process {learnersToProcessUln.Count}");

            //1. Get all Learners From Approvals in batches
            DateTime? extractStartTime = new DateTime(2000, 1, 1);
            const int batchSize = 1000;
            int batchNumber = 0;

            GetAllLearnersResponse learnersResponse = await _outerApiClient.Get<GetAllLearnersResponse>(new GetAllLearnersRequest(extractStartTime, batchNumber, batchSize));
            if (learnersResponse?.Learners == null)
            {
                throw new Exception($"Failed to get learners batch: sinceTime={extractStartTime?.ToString("o", System.Globalization.CultureInfo.InvariantCulture)} batchNumber={batchNumber} batchSize={batchSize}");
            }

            // 2. Check if the learner needs to be processed if yes enqueue
            foreach (var learner in learnersResponse.Learners)
            {

                long uln = 0;
                int trainingCode = 0;

                long.TryParse(learner.ULN, out uln);
                int.TryParse(learner.TrainingCode, out trainingCode);

                if (learnersToProcessUln.Contains(long.Parse(learner.ULN)))
                {
                    var message = new UpdateLearnersInfoMessage(
                        learner.EmployerAccountId,
                        learner.EmployerName,
                        uln,
                        trainingCode);

                    learnersToUpdate.Add(message);
                }
            }

            return learnersToUpdate;
        }

        public async Task<List<UpdateLearnersInfoMessage>> ProcessLearners(List<UpdateLearnersInfoMessage> learnersInfoMessages)
        {
            if (learnersInfoMessages == null)
                return new List<UpdateLearnersInfoMessage>();

            var learners = learnersInfoMessages
                .Select(learner => (learner.Uln, learner.StdCode, learner.EmployerAccountId, learner.EmployerName))
                .ToList();

            await _assessorServiceRepository.UpdateLeanerInfo(learners);

            var nextLeanerBatch = await GetLearnersToUpdate();

            return nextLeanerBatch;
        }

    }
}
