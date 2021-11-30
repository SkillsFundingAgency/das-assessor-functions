using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Services
{
    public class RefreshIlrsAcademicYearService : IRefreshIlrsAcademicYearService
    {
        private readonly RefreshIlrsOptions _refreshIlrsOptions;
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly ILogger<RefreshIlrsAcademicYearService> _logger;

        public RefreshIlrsAcademicYearService(IOptions<RefreshIlrsOptions> options, IDataCollectionServiceApiClient dataCollectionServiceApiClient,
            ILogger<RefreshIlrsAcademicYearService> logger)
        {
            _refreshIlrsOptions = options?.Value;
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _logger = logger;
        }

        public async Task<List<string>> ValidateAllAcademicYears(DateTime lastRunDateTime, DateTime currentRunDateTime)
        {
            IEnumerable<string> sources = null;
            try
            {
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
                await Task.WhenAll(sourceValidations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to validate academic years between {lastRunDateTime.ToShortDateString()} and {currentRunDateTime.ToShortDateString()}");
                throw;
            }

            return sources?.ToList();
        }

        private async Task ValidateAcademicYear(string source)
        {
            try
            {
                // check whether there is a valid source endpoint in the data collection API
                var providersPage = await _dataCollectionServiceApiClient.GetProviders(source, DateTime.MaxValue, 1, 1);
                if (providersPage.Providers.Count > 0 || providersPage.PagingInfo.TotalItems > 0)
                {
                    // no content should exists for any providers in the future 
                    throw new Exception($"Academic year {source} contains future records");
                }
            }
            catch (Exception ex)
            {
                // any unexpected failure (e.g. 404 not found) indicates that the source endpoint cannot be reached in the data collection API
                _logger.LogError(ex, $"Invalid academic year {source}");
                throw;
            }
        }
    }
}
