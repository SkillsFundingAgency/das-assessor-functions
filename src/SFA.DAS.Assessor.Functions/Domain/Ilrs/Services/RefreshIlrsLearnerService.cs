﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Services
{
    public class RefreshIlrsLearnerService : IRefreshIlrsLearnerService
    {
        private readonly RefreshIlrsSettings _refreshIlrsOptions;
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;
        private readonly ILogger<RefreshIlrsLearnerService> _logger;

        public RefreshIlrsLearnerService(IOptions<RefreshIlrsSettings> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient, 
            IAssessorServiceApiClient assessorServiceApiClient, ILogger<RefreshIlrsLearnerService> logger)
        {
            _refreshIlrsOptions = options?.Value;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assessorServiceApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public async Task<RefreshIlrsProviderMessage> ProcessLearners(RefreshIlrsProviderMessage providerMessage)
        {
            try
            {
                return await ExportLearnerDetails(providerMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Refresh Ilrs for Ukprn {providerMessage.Ukprn}, process learners failed for '{providerMessage.Source}' on page {providerMessage.LearnerPageNumber} ");
                throw;
            }
        }

        private async Task<RefreshIlrsProviderMessage> ExportLearnerDetails(RefreshIlrsProviderMessage providerMessage)
        {
            RefreshIlrsProviderMessage nextPageProviderMessage = null;
            
            var aimType = 1;
            var allStandards = -1;
            var fundModels = ConfigurationHelper.ConvertCsvValueToList<int>(_refreshIlrsOptions.LearnerFundModels);
            var pageSize = _refreshIlrsOptions.LearnerPageSize;

            var learnersPage = await _dataCollectionServiceApiClient.GetLearners(providerMessage.Source, providerMessage.Ukprn, aimType, allStandards, fundModels, pageSize, providerMessage.LearnerPageNumber);
            if (learnersPage != null)
            {
                if (learnersPage.PagingInfo.TotalPages > learnersPage.PagingInfo.PageNumber)
                {
                    // queue a new message to process the next page
                    nextPageProviderMessage = new RefreshIlrsProviderMessage
                    {
                        Ukprn = providerMessage.Ukprn,
                        Source = providerMessage.Source,
                        LearnerPageNumber = providerMessage.LearnerPageNumber + 1
                    };
                }

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
                            _logger.LogDebug($"Request to import learner details failed due to '{ld.Outcome}' '{e}'");
                        });
                    });
                }
            }

            return nextPageProviderMessage;
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