using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Services
{
    public class RefreshIlrsProviderService : IRefreshIlrsProviderService
    {
        private readonly RefreshIlrsOptions _refreshIlrsOptions;
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assessorApiClient;
        private readonly IDateTimeHelper _dateTimeHelper;
        
        private readonly ILogger<RefreshIlrsProviderService> _logger;

        private const string RefreshIlrsLastRunDate = "RefreshIlrsLastRunDate";

        public RefreshIlrsProviderService(IOptions<RefreshIlrsOptions> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient, 
            IAssessorServiceApiClient assessorServiceApiClient, IDateTimeHelper dateTimeHelper, ILogger<RefreshIlrsProviderService> logger)
        {
            _refreshIlrsOptions = options?.Value;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assessorApiClient = assessorServiceApiClient;
            _dateTimeHelper = dateTimeHelper;
            _logger = logger;
        }

        public async Task<List<RefreshIlrsProviderMessage>> ProcessProviders()
        {           
            var lastRunDateTime = await GetLastRunDateTime();
            var currentDateTime = _dateTimeHelper.DateTimeNow;

            // the sources that are valid either at the last run time or the current time are combined
            // and validated; if they are ALL valid then the providers which have changed since the last
            // run time will be queued for processing learner details
            var validSources = await ValidateAllAcademicYears(lastRunDateTime, currentDateTime);
            if (!validSources.Any())
            {
                _logger.LogError($"Refresh Ilrs enqueue providers failed, invalid source or none between {lastRunDateTime} to {currentDateTime}");
            }
            else
            {
                var providerMessagesToQueue = new List<RefreshIlrsProviderMessage>();
                foreach (var source in validSources)
                {
                    try
                    {
                        providerMessagesToQueue.AddRange(await QueueProviders(source, lastRunDateTime));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Refresh Ilrs enqueue providers failed for academic year {source}");

                        // if any source encouters an unexpected failure - the queue process will be aborted 
                        // and will repeat for ALL sources the next time it is scheduled, duplication by repeating 
                        // a successfully queued source is preferred to missing any updates.
                        throw;
                    }
                }
                
                return providerMessagesToQueue;
            }

            return null;
        }

        public async Task<DateTime> GetLastRunDateTime()
        {
            try
            {
                var lastRunDateTimeSetting = await _assessorApiClient.GetAssessorSetting(RefreshIlrsLastRunDate);
                if (lastRunDateTimeSetting != null)
                {
                    if (DateTime.TryParse(lastRunDateTimeSetting, out DateTime lastRunDateTime))
                        return lastRunDateTime;
                }
            }
            catch
            {
                _logger.LogInformation($"There is no {RefreshIlrsLastRunDate}, using default last run date {_refreshIlrsOptions.ProviderInitialRunDate}");
            }

            return _refreshIlrsOptions.ProviderInitialRunDate;
        }

        public async Task SetLastRunDateTime(DateTime nextRunDateTime)
        {
            await _assessorApiClient.SetAssessorSetting(RefreshIlrsLastRunDate, nextRunDateTime.ToString("o"));
        }

        private async Task<List<string>> ValidateAllAcademicYears(DateTime lastRunDateTime, DateTime currentRunDateTime)
        {
            IEnumerable<string> sources;
            if (string.IsNullOrEmpty(_refreshIlrsOptions.AcademicYearsOverride))
            {
                var sourcesLast = await _dataCollectionServiceApiClient.GetAcademicYears(lastRunDateTime);
                var sourceCurrent = await _dataCollectionServiceApiClient.GetAcademicYears(currentRunDateTime);

                sources = sourcesLast
                    .Union(sourceCurrent)
                    .Distinct();
            }
            else
            {
                sources = ConfigurationHelper.ConvertCsvValueToList<string>(_refreshIlrsOptions.AcademicYearsOverride);
            }

            var sourceValidations = sources.Select(source => ValidateAcademicYear(source));
            bool[] results = await Task.WhenAll(sourceValidations);
            if(results.All(item => item))
            {
                return sources.ToList();
            }

            return new List<string>();
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
                    throw new Exception($"Refresh Ilrs enqueue providers academic year {source} contains future records");
                }
                
                return true;
            }
            catch(Exception ex)
            {
                // any unexpected failure (e.g. 404 not found) indicates that the source endpoint cannot be reached in the data collection API
                _logger.LogError(ex, $"Refresh Ilrs enqueue providers invalid academic year {source}");
            }

            return false;
        }

        private async Task<List<RefreshIlrsProviderMessage>> QueueProviders(string source, DateTime lastRunDateTime)
        {
            var providerMessagesToQueue = new List<RefreshIlrsProviderMessage>();
            var pageSize = _refreshIlrsOptions.ProviderPageSize;

            var providersPage = await _dataCollectionServiceApiClient.GetProviders(source, lastRunDateTime, pageSize, pageNumber: 1);
            if (providersPage != null && providersPage.PagingInfo.TotalItems > 0)
            {
                do
                {
                    foreach (var providerUkprn in providersPage.Providers)
                    {
                        var message = new RefreshIlrsProviderMessage
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
