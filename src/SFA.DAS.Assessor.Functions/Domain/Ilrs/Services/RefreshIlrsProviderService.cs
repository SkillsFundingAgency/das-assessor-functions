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
        private readonly IRefreshIlrsAcademicYearService _refreshIlrsAcademicYearService;
        private readonly ILogger<RefreshIlrsProviderService> _logger;

        public RefreshIlrsProviderService(IOptions<RefreshIlrsOptions> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient, 
            IRefreshIlrsAcademicYearService refreshIlrsAcademicYearService, ILogger<RefreshIlrsProviderService> logger)
        {
            _refreshIlrsOptions = options?.Value;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _refreshIlrsAcademicYearService = refreshIlrsAcademicYearService;
            _logger = logger;
        }

        public async Task<List<RefreshIlrsProviderMessage>> ProcessProviders(DateTime lastRunDateTime, DateTime currentRunDateTime)
        {
            // the sources that are valid either at the last run time or the current time are combined
            // and validated; if they are ALL valid then the providers which have changed since the last
            // run time will be queued for processing learner details
            try
            {
                var validSources = await _refreshIlrsAcademicYearService.ValidateAllAcademicYears(lastRunDateTime, currentRunDateTime);
                if (!validSources.Any())
                {
                    throw new Exception($"Invalid source or none between {lastRunDateTime} and {currentRunDateTime}");
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
                            // if any source encouters an unexpected failure - the queue process will be aborted 
                            // and will repeat for ALL sources the next time it is scheduled, duplication by repeating 
                            // a successfully queued source is preferred to missing any updates.
                            throw new Exception($"Unable to queue providers for academic year {source}", ex);
                        }
                    }

                    return providerMessagesToQueue;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Unable to process providers between {lastRunDateTime} and {currentRunDateTime}");
            }

            return null;
        }

        private async Task<List<RefreshIlrsProviderMessage>> QueueProviders(string source, DateTime startDateTime)
        {
            var providerMessagesToQueue = new List<RefreshIlrsProviderMessage>();
            var pageSize = _refreshIlrsOptions.ProviderPageSize;

            var providersPage = await _dataCollectionServiceApiClient.GetProviders(source, startDateTime, pageSize, pageNumber: 1);
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
                    providersPage = await _dataCollectionServiceApiClient.GetProviders(source, startDateTime, pageSize, providersPage.PagingInfo.PageNumber + 1);
                }
                while (providersPage != null && providersPage.PagingInfo.PageNumber <= providersPage.PagingInfo.TotalPages);
            }

            return providerMessagesToQueue;
        }
    }
}
