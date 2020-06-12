using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Services
{
    public class EpaoDataSyncProviderService : IEpaoDataSyncProviderService
    {
        private readonly EpaoDataSyncSettings _epaoDataSyncOptions;
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assessorApiClient;
        
        private readonly ILogger<EpaoDataSyncProviderService> _logger;

        public EpaoDataSyncProviderService(IOptions<EpaoDataSyncSettings> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient, 
            IAssessorServiceApiClient assessorServiceApiClient, ILogger<EpaoDataSyncProviderService> logger)
        {
            _epaoDataSyncOptions = options?.Value;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assessorApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public async Task<List<EpaoDataSyncProviderMessage>> ProcessProviders()
        {
            var providerMessagesToQueue = new List<EpaoDataSyncProviderMessage>();
            _logger.LogDebug($"Using data collection api base address: {_dataCollectionServiceApiClient.BaseAddress()}");

            var lastRunDateTime = await GetLastRunDateTime();

            // the sources which are valid at the last run datetime are obtained to ensure that extended
            // periods of downtime do not result in missing updates for previous academic years
            var sources = await _dataCollectionServiceApiClient.GetAcademicYears(lastRunDateTime);
            foreach (var source in sources ?? new List<string>())
            {
                // process all the sources for which there is a valid endpoint in the data collection API
                if (await ValidateAcademicYear(source))
                {
                    try
                    {
                        providerMessagesToQueue.AddRange(await QueueProviders(source, lastRunDateTime));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Epao data sync enqueue providers failed for academic year {source}");

                        // if any source encouters an unexpected failure - the queue process will be aborted 
                        // and will repeat for ALL sources the next time it is scheduled, duplication by repeating 
                        // a successfully queued source is preferred to missing any updates.
                        throw;
                    }
                }
            }

            return providerMessagesToQueue;
        }

        public async Task<DateTime> GetLastRunDateTime()
        {
            try
            {
                var lastRunDateTimeSetting = await _assessorApiClient.GetAssessorSetting("EpaoDataSyncLastRunDate");
                if (lastRunDateTimeSetting != null)
                {
                    if (DateTime.TryParse(lastRunDateTimeSetting, out DateTime lastRunDateTime))
                        return lastRunDateTime;
                }
            }
            catch
            {
                _logger.LogInformation($"There is no EpaoDataSyncLastRunDate, using default last run date {_epaoDataSyncOptions.ProviderInitialRunDate}");
            }

            return _epaoDataSyncOptions.ProviderInitialRunDate;
        }

        public async Task SetLastRunDateTime(DateTime nextRunDateTime)
        {
            await _assessorApiClient.SetAssessorSetting("EpaoDataSyncLastRunDate", nextRunDateTime.ToString("o"));
        }

        private async Task<bool> ValidateAcademicYear(string source)
        {
            try
            {
                // check whether there is a valid source endpoint in the data collection API
                var providersPage = await _dataCollectionServiceApiClient.GetProviders(source, DateTime.MaxValue, 1, 1);
                if (providersPage.Providers.Count > 0 || providersPage.PagingInfo.TotalItems > 0)
                {
                    // no content should exists for any providers in the future 
                    throw new Exception($"Epao data sync enqueue providers academic year {source} contains future records");
                }
                
                return true;
            }
            catch(Exception ex)
            {
                // any unexpected failure (e.g. 404 not found) indicates that the source endpoint cannot be reached in the data collection API
                _logger.LogError(ex, $"Epao data sync enqueue providers invalid academic year {source}");
            }

            return false;
        }

        private async Task<List<EpaoDataSyncProviderMessage>> QueueProviders(string source, DateTime lastRunDateTime)
        {
            var providerMessagesToQueue = new List<EpaoDataSyncProviderMessage>();
            var pageSize = _epaoDataSyncOptions.ProviderPageSize;

            var providersPage = await _dataCollectionServiceApiClient.GetProviders(source, lastRunDateTime, pageSize, pageNumber: 1);
            if (providersPage != null && providersPage.PagingInfo.TotalItems > 0)
            {
                do
                {
                    foreach (var providerUkprn in providersPage.Providers)
                    {
                        var message = new EpaoDataSyncProviderMessage
                        {
                            Ukprn = providerUkprn,
                            Source = source,
                            LearnerPageNumber = 1
                        };

                        providerMessagesToQueue.Add(message);
                    }

                    // each subsequent page will be retrieved; any data which has changed during paging which would be contained 
                    // on a previously retrieved page will be proceseed on a subsequent run; any data which has changed during paging
                    // which would be contained on a subsequent page will be duplicated on a subsequent run;
                    providersPage = await _dataCollectionServiceApiClient.GetProviders(source, lastRunDateTime, pageSize, providersPage.PagingInfo.PageNumber + 1);
                }
                while (providersPage != null && providersPage.PagingInfo.PageNumber <= providersPage.PagingInfo.TotalPages);
            }

            return providerMessagesToQueue;
        }
    }
}
