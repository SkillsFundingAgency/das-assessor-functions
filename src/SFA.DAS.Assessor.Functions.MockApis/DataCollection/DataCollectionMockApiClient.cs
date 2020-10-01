using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.MockApis.DataCollection.DataGenerator;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection
{
    public class DataCollectionMockApiClient : IDataCollectionServiceApiClient
    {
        private ILogger<DataCollectionMockApiClient> _logger;
        private List<string> _academicYears;
        private List<DataCollectionLearner> _learnerMockData = new List<DataCollectionLearner>();
        private List<int> _providerMockData = new List<int>();

        public DataCollectionMockApiClient(IOptions<DataCollectionMock> options, ILogger<DataCollectionMockApiClient> logger)
        {
            _logger = logger;

            _academicYears = new List<string>() { options.Value.AcademicYear };
            SetupDataCollectionMockData(options.Value.ProviderCount, options.Value.LearnerCount, options.Value.LearningDeliveryCount);
        }

        private void SetupDataCollectionMockData(int providerCount, int learnerCount, int learningDeliveryCount)
        {
            var generatingInfo = $"generating {providerCount} providers for {learnerCount} learners with {learningDeliveryCount} learning deliveries from source(s) {string.Join(",", _academicYears.ToArray())}";
            _logger.LogInformation($"Epao RefreshIlrsEnqueueProviders DataCollectionMockApiClient started {generatingInfo}");

            var generator = new DataCollectionMockDataGenerator(1, new List<int> { 36, 81, 99 });
            for (int count = 0; count < providerCount; count++)
            {
                var provider = Providers.ProvidersList[count];
                _learnerMockData.AddRange(generator.GetLearners(provider, learnerCount, learningDeliveryCount));
            }

            _providerMockData.AddRange(Providers.ProvidersList.Take(providerCount).ToList());

            _logger.LogInformation($"Epao RefreshIlrsEnqueueProviders DataCollectionMockApiClient finished {generatingInfo}");

        }

        public string BaseAddress()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAcademicYears(DateTime dateTimeUtc)
        {
            return Task.FromResult(_academicYears);
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
