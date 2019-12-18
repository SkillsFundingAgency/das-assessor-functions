using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncDequeueProviders
    {
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assesorServiceApiClient;
        private readonly ILogger<EpaoDataSyncDequeueProviders> _logger;

        public EpaoDataSyncDequeueProviders(IDataCollectionServiceApiClient dataCollectionServiceApiClient, IAssessorServiceApiClient assessorServiceApiClient, ILogger<EpaoDataSyncDequeueProviders> logger)
        {
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assesorServiceApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        [FunctionName("EpaoDataSyncDequeueProviders")]
        public async Task Run([QueueTrigger("epao-data-sync-providers", Connection = "ConfigurationStorageConnectionString")]string message, ILogger log)
        {
            _logger.LogDebug($"Epao data sync dequeue provider function triggered for: {message}");

            _logger.LogDebug($"Using data collection api base address: {_dataCollectionServiceApiClient.BaseAddress()}");
            _logger.LogDebug($"Using assessor api base address: {_assesorServiceApiClient.BaseAddress()}");

            var providerMessage = JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(message);

            try
            {
                var learnersExported = false;
                while (!learnersExported)
                {
                    try
                    {
                        await ExportLearnerDetails(providerMessage);
                        learnersExported = true;
                    }
                    catch (PagingInfoChangedException)
                    {
                        // the export process will be restarted when learners have changed whilst paging
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Epao data sync dequeue providers function failed");
            }

            _logger.LogDebug("Epao data sync dequeue provider function completed");
        }

        private async Task ExportLearnerDetails(EpaoDataSyncProviderMessage providerMessage)
        {
            const int pageSize = 2; // TODO: increase to large value e.g. 100 for production to increase throughput

            var allStandards = -1;
            var aimType = 1;
            var fundModels = new List<int> { 36, 99, 81 };

            var learnersPage = await _dataCollectionServiceApiClient.GetLearners(providerMessage.Source, providerMessage.Ukprn, aimType, allStandards, fundModels, pageSize, pageNumber: 1);
            if (learnersPage != null)
            {
                do
                {
                    var filteredLearners = learnersPage.Learners
                    .SelectMany(p => p.LearningDeliveries, (l, ld) => new
                    {
                        Learner = l,
                        LearningDelivery = ld
                    })
                    .Where(p =>
                        p.LearningDelivery.AimType == aimType &&
                        p.LearningDelivery.StdCode != null &&
                        (p.LearningDelivery.FundModel.HasValue && fundModels.Contains(p.LearningDelivery.FundModel.Value)))
                    .Select(p => new ImportLearnerDetail
                    {
                        Source = providerMessage.Source,
                        Ukprn = p.Learner.Ukprn,
                        Uln = p.Learner.Uln,
                        StdCode = p.LearningDelivery.StdCode,
                        FundingModel = p.LearningDelivery.FundModel,
                        GivenNames = p.Learner.GivenNames,
                        FamilyName = p.Learner.FamilyName,
                        EpaOrgId = p.LearningDelivery.EpaOrgID,
                        LearnStartDate = p.LearningDelivery.LearnStartDate,
                        PlannedEndDate = p.LearningDelivery.LearnPlanEndDate,
                        CompletionStatus = p.LearningDelivery.CompStatus,
                        LearnRefNumber = p.Learner.LearnRefNumber,
                        DelLocPostCode = p.LearningDelivery.DelLocPostCode,
                        LearnActEndDate = p.LearningDelivery.LearnActEndDate,
                        WithdrawReason = p.LearningDelivery.WithdrawReason,
                        Outcome = p.LearningDelivery.Outcome,
                        AchDate = p.LearningDelivery.AchDate,
                        OutGrade = p.LearningDelivery.OutGrade
                    });

                    ImportLearnerDetailRequest request = new ImportLearnerDetailRequest
                    {
                        ImportLearnerDetails = filteredLearners.ToList()
                    };

                    var response = await _assesorServiceApiClient.ImportLearnerDetails(request);
                    if(response?.LearnerDetailResults != null)
                    {
                        foreach(var result in response.LearnerDetailResults)
                        {
                            if (result.Errors != null)
                            {
                                foreach (var error in result.Errors)
                                {
                                    _logger.LogDebug($"Unable to export Learner Details due to '{result.Outcome}' '{error}'");
                                }
                            }
                        }
                    }

                    var nextLearnersPage = await _dataCollectionServiceApiClient.GetLearners(providerMessage.Source, providerMessage.Ukprn, aimType, allStandards, fundModels, pageSize, learnersPage.PagingInfo.PageNumber + 1);
                    if (nextLearnersPage != null)
                    {
                        if (nextLearnersPage.PagingInfo.TotalItems != learnersPage.PagingInfo.TotalItems || nextLearnersPage.PagingInfo.TotalPages != learnersPage.PagingInfo.TotalPages)
                        {
                            // if the total number of items or pages has changed then the process will need to be restarted to 
                            // avoid skipping any updated providers on earlier pages
                            throw new PagingInfoChangedException();
                        }
                    }

                    learnersPage = nextLearnersPage;
                }
                while (learnersPage != null && learnersPage.PagingInfo.PageNumber <= learnersPage.PagingInfo.TotalPages);
            }
        }
    }
}
