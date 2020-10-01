using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using SFA.DAS.Assessor.Functions.MockApis.DataCollection.DataGenerator;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection
{
    /// <summary>
    /// The amount of Mock data which is generated on startup is controlled by the Settings table in the Assessor database, the RefreshIlrsLastRunDate
    /// is passed to the GetAcademicYears method, depending on the value in the Settings:
    /// 
    /// YYYY-06-15 (10 Providers, 4 Learners, 1 Valid Learning Delivery and upto 4 invalid learning deliveries)
    /// YYYY-05-15 (100 Providers, 8 Learners, 2 Valid Learning Delivery and upto 4 invalid learning deliveries)
    /// YYYY-04-15 (200 Providers, 16 Learners, 3 Valid Learning Delivery and upto 4 invalid learning deliveries)
    /// YYYY-03-15 (400 Providers, 32 Learners, 4 Valid Learning Delivery and upto 4 invalid learning deliveries)
    /// YYYY-02-15 (800 Providers, 64 Learners, 5 Valid Learning Delivery and upto 4 invalid learning deliveries)
    /// YYYY-01-15 (1600 Providers, 128 Learners, 6 Valid Learning Delivery and upto 4 invalid learning deliveries)
    /// 
    /// 1000 = Academic Year 1920
    /// 2000 = Academic Year 2021
    ///
    /// Notes:
    /// 1) Always use a time in the RefreshIlrsLastRunDate which is 12:00 to avoid any UTC time shift to a previous day
    /// 
    /// 2) The amount of data sent to the Assessor and stored in the [Ilrs] table is not a straight multiplication of the above numbers
    /// due to there being overlapping sets of data which can cause Updates rather than inserts and data which may be ignored due to 
    /// business rules, the purpose of the Mock data is to test the performance NOT the correctness of the data.
    /// 
    /// </summary>
    public class DataCollectionMockApiClient : IDataCollectionServiceApiClient
    {
        private List<DataCollectionLearner> _learnerMockData = null;
        private List<int> _providerMockData = null;

        ILogger<DataCollectionMockApiClient> _logger;

        public DataCollectionMockApiClient(ILogger<DataCollectionMockApiClient> logger)
        {
            _logger = logger;
        }

        private void AddDataCollectionLearnersForStartDate(int providerCount, int learnerCount, int learningDeliveryCount)
        {
            _learnerMockData = new List<DataCollectionLearner>();

            var generator = new DataCollectionMockDataGenerator(1, new List<int> { 36, 81, 99 });
            for (int count = 0; count < providerCount; count++)
            {
                var provider = Providers.ProvidersList[count];
                _learnerMockData.AddRange(generator.GetLearners(provider, learnerCount, learningDeliveryCount));
            }
        }
        
        public string BaseAddress()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAcademicYears(DateTime dateTimeUtc)
        {
            // initialize the test data for this singleton instance on the first call to academic years
            if (_learnerMockData == null || _providerMockData == null)
            {
                ProviderCountDates.GetCounts(dateTimeUtc.Date, out int providerCount, out int learnerCount, out int learningDeliveryCount);
                _logger.LogInformation($"Epao RefreshIlrsEnqueueProviders DataCollectionMockApiClient will generate {providerCount} providers for {dateTimeUtc.Date} with {learnerCount} learners and {learningDeliveryCount} learning deliveries");

                AddDataCollectionLearnersForStartDate(providerCount, learnerCount, learningDeliveryCount);

                _providerMockData = new List<int>();
                _providerMockData.AddRange(Providers.ProvidersList.Take(providerCount).ToList());

                _logger.LogInformation($"Epao RefreshIlrsEnqueueProviders DataCollectionMockApiClient has generated {providerCount} providers for {dateTimeUtc.Date} with {learnerCount} learners and {learningDeliveryCount} learning deliveries");
            }

            switch (dateTimeUtc.Year)
            {
                case 1000:
                    return Task.FromResult(new List<string>() { "1920" });
                    
                case 2000:
                    return Task.FromResult(new List<string>() { "2021" });

                default: 
                    return Task.FromResult(new List<string>() { "1920" });
            }
        }

        public Task<DataCollectionLearnersPage> GetLearners(string source, DateTime startDateTime, int? aimType, int? standardCode, List<int> fundModels, int? pageSize, int? pageNumber)
        {
            var learners = _learnerMockData;

            var leanersPage = learners
                .Skip((pageSize ?? 10) * (pageNumber-1) ?? 1).Take(pageSize ?? 10);

            var page = new DataCollectionLearnersPage()
            {
                Learners = new List<DataCollectionLearner>(leanersPage),
                PagingInfo = new DataCollectionPagingInfo()
                {
                    PageNumber = pageNumber ?? 1,
                    PageSize = pageSize ?? 10,
                    TotalItems = learners.Count(),
                    TotalPages = Math.Max((learners.Count() / pageSize ?? 10), 1)
                }
            };
            
            return Task.FromResult(page);
        }

        public Task<DataCollectionLearnersPage> GetLearners(string source, int ukprn, int? aimType, int? standardCode, List<int> fundModels, int? pageSize, int? pageNumber)
        {
            var learners = _learnerMockData
                .Where(p => p.Ukprn == ukprn);

            var learnersPage = learners
                .Skip((pageSize ?? 10) * (pageNumber - 1) ?? 1).Take(pageSize ?? 10);

            var page = new DataCollectionLearnersPage()
            {
                Learners = new List<DataCollectionLearner>(learnersPage),
                PagingInfo = new DataCollectionPagingInfo()
                {
                    PageNumber = pageNumber ?? 1,
                    PageSize = pageSize ?? 10,
                    TotalItems = learners.Count(),
                    TotalPages = Math.Max((learners.Count() / pageSize ?? 10), 1)
                }
            };

            return Task.FromResult(page);
        }

        public Task<DataCollectionProvidersPage> GetProviders(string source, DateTime startDateTime, int? pageSize, int? pageNumber)
        {
            var providers = startDateTime.Date < DateTime.Now.Date
                ? _providerMockData
                : new List<int>();

            var providersPage = providers
                .Skip((pageSize ?? 10) * (pageNumber - 1) ?? 1).Take(pageSize ?? 10);

            var page = new DataCollectionProvidersPage()
            {
                Providers = new List<int>(providersPage),
                PagingInfo = new DataCollectionPagingInfo()
                {
                    PageNumber = pageNumber ?? 1,
                    PageSize = pageSize ?? 10,
                    TotalItems = providers.Count(),
                    TotalPages = Math.Max((providers.Count() / pageSize ?? 10), 1)
                }
            };

            return Task.FromResult(page);
        }
    }
}
