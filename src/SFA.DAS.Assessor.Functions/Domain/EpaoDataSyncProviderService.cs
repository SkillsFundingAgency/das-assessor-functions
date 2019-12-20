using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.Exceptions;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public class EpaoDataSyncProviderService : IEpaoDataSyncProviderService
    {
        private readonly IOptions<EpaoDataSync> _options;
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IAssessorServiceApiClient _assessorApiClient;
        private readonly IStorageQueueService _storageQueueService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILogger<EpaoDataSyncProviderService> _logger;

        public EpaoDataSyncProviderService(IOptions<EpaoDataSync> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient, 
            IAssessorServiceApiClient assessorServiceApiClient, IStorageQueueService storageQueueService, IDateTimeHelper dateTimeHelper, ILogger<EpaoDataSyncProviderService> logger)
        {
            _options = options;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _assessorApiClient = assessorServiceApiClient;
            _storageQueueService = storageQueueService;
            _dateTimeHelper = dateTimeHelper;
            _logger = logger;
        }

        public async Task ProcessProviders()
        {
            _logger.LogDebug($"Using data collection api base address: {_dataCollectionServiceApiClient.BaseAddress()}");

            var lastRunDateTime = await GetLastRunDateTime();
            var nextRunDateTime = _dateTimeHelper.DateTimeNow;

            var sources = await _dataCollectionServiceApiClient.GetAcademicYears(_dateTimeHelper.DateTimeUtcNow); 
            foreach (var source in sources)
            {
                // process all the sources for which there is a valid endpoint in the data collection API
                if (await ValidateAcademicYear(source))
                {
                    try
                    {
                        var providersQueued = false;
                        while (!providersQueued)
                        {
                            try
                            {
                                await QueueProviders(source, lastRunDateTime);
                                providersQueued = true;
                            }
                            catch (PagingInfoChangedException)
                            {
                                // the queue process will be restarted when providers have changed whilst queueing but the 
                                // next run date will be advanced to avoid duplicating them on the next run
                                nextRunDateTime = _dateTimeHelper.DateTimeNow;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Epao data sync enqueue providers failed for academic year {source}");

                        // if any source encouters an unexpected failure - all sources will be repeated, duplication
                        // by repeating a source is preferred to missing any updates.
                        throw;
                    }
                }
            }

            // when all sources were processed successfully store the date for the next run
            await _assessorApiClient.SetAssessorSetting("EpaoDataSyncLastRunDate", nextRunDateTime.ToString("u"));
        }

        private async Task<DateTime> GetLastRunDateTime()
        {            
            var lastRunDateTimeSetting = await _assessorApiClient.GetAssessorSetting("EpaoDataSyncLastRunDate");
            if(lastRunDateTimeSetting != null)
            {
                if(DateTime.TryParse(lastRunDateTimeSetting, out DateTime lastRunDateTime))
                    return lastRunDateTime;
            }

            return _options.Value.ProviderInitialRunDate;
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

        private async Task QueueProviders(string source, DateTime lastRunDateTime)
        {
            var pageSize = _options.Value.ProviderPageSize;

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
                            Source = source
                        };

                        var jsonMessage = JsonConvert.SerializeObject(message);
                        await _storageQueueService.AddMessageAsync(new CloudQueueMessage(jsonMessage));
                    }

                    var nextProvidersPage = await _dataCollectionServiceApiClient.GetProviders(source, lastRunDateTime, pageSize, providersPage.PagingInfo.PageNumber + 1);
                    if (nextProvidersPage != null)
                    {
                        if (nextProvidersPage.PagingInfo.TotalItems != providersPage.PagingInfo.TotalItems || nextProvidersPage.PagingInfo.TotalPages != providersPage.PagingInfo.TotalPages)
                        {
                            // if the total number of items or pages has changed then the process will need to be restarted to 
                            // avoid skipping any updated providers on earlier pages
                            throw new PagingInfoChangedException();
                        }
                    }

                    providersPage = nextProvidersPage;
                }
                while (providersPage != null && providersPage.PagingInfo.PageNumber <= providersPage.PagingInfo.TotalPages);
            }
        }
    }
}
