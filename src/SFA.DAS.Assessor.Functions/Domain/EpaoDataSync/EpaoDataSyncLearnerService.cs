using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Config;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public class EpaoDataSyncLearnerService : IEpaoDataSyncLearnerService
    {
        private readonly IOptions<EpaoDataSync> _options;
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;
        private readonly ILogger<EpaoDataSyncLearnerService> _logger;

        public EpaoDataSyncLearnerService(IOptions<EpaoDataSync> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient, 
            IAssessorServiceApiClient assessorServiceApiClient, ILogger<EpaoDataSyncLearnerService> logger)
        {
            _options = options;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assessorServiceApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public async Task ProcessLearners(EpaoDataSyncProviderMessage providerMessage)
        {
            try
            {
                _logger.LogDebug($"Using data collection api base address: {_dataCollectionServiceApiClient.BaseAddress()}");
                _logger.LogDebug($"Using assessor api base address: {_assessorServiceApiClient.BaseAddress()}");

                var learnersExported = false;
                while (!learnersExported)
                {
                    await ExportLearnerDetails(providerMessage);
                    learnersExported = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Epao data sync process learners failed");
                throw;
            }
        }

        private async Task ExportLearnerDetails(EpaoDataSyncProviderMessage providerMessage)
        {
            var aimType = 1;
            var allStandards = -1;
            var fundModels = ConfigHelper.ConvertCsvValueToList<int>(_options.Value.LearnerFundModels);
            var pageSize = _options.Value.LearnerPageSize;

            var learnersPage = await _dataCollectionServiceApiClient.GetLearners(providerMessage.Source, providerMessage.Ukprn, aimType, allStandards, fundModels, pageSize, pageNumber: 1);
            if (learnersPage != null)
            {
                do
                {
                    // the learners must be filtered to remove learning deliveries which do not match the filters as the 
                    // data collection API returns learners which have at least one learning delivery matching a filter, 
                    // but it then returns ALL learning deliveries for each matching learner
                    var filteredLearners = FilterLearners(learnersPage.Learners, providerMessage.Source, aimType, fundModels);
                    if (filteredLearners.Count > 0)
                    {
                        ImportLearnerDetailRequest request = new ImportLearnerDetailRequest
                        {
                            ImportLearnerDetails = filteredLearners
                        };

                        var response = await _assessorServiceApiClient.ImportLearnerDetails(request);
                        response?.LearnerDetailResults?.ForEach(ld =>
                        {
                            ld?.Errors?.ForEach(e =>
                            {
                                _logger.LogDebug($"Unable to export Learner Details due to '{ld.Outcome}' '{e}'");
                            });
                        });
                    }

                    learnersPage = await _dataCollectionServiceApiClient.GetLearners(providerMessage.Source, providerMessage.Ukprn, aimType, allStandards, fundModels, pageSize, learnersPage.PagingInfo.PageNumber + 1); ;
                }
                while (learnersPage != null && learnersPage.PagingInfo.PageNumber <= learnersPage.PagingInfo.TotalPages);
            }
        }

        private List<ImportLearnerDetail> FilterLearners(List<DataCollectionLearner> dataCollectionLearners, string source, int aimType, List<int> fundModels)
        {
            return dataCollectionLearners
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
                        Source = source,
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
                    })
                    .ToList();
        }
    }
}
