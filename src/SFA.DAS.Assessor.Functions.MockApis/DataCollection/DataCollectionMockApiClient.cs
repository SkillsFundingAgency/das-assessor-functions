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
        private IOptions<DataCollectionMock> _optionsDataCollectionMock;
        private ILogger<DataCollectionMockApiClient> _logger;
       
        private List<int> _providerMockData = new List<int>();
        private List<DataCollectionLearner> _learnerMockDataList = new List<DataCollectionLearner>();
        private Dictionary<int, List<DataCollectionLearner>> _learnerMockDataDictionary = new Dictionary<int, List<DataCollectionLearner>>();

        private DataCollectionMockDataGenerator _generator;
        
        public DataCollectionMockApiClient(IOptions<DataCollectionMock> optionsDataCollectionMock,
            IOptions<RefreshIlrsSettings> optionsRefeshIlrs,
            ILogger<DataCollectionMockApiClient> logger)
        {
            _logger = logger;

            _optionsDataCollectionMock = optionsDataCollectionMock;

            _providerMockData.AddRange(Providers.ProvidersList.Take(_optionsDataCollectionMock.Value.ProviderCount).ToList());

            var aimType = 1;
            var fundModels = optionsRefeshIlrs.Value.LearnerFundModels;
            
            _generator = new DataCollectionMockDataGenerator(aimType, 
                new List<string>(fundModels.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .ConvertAll<int>(p => int.Parse(p.Trim())));

            var generatingInfo = $"generating {_optionsDataCollectionMock.Value.ProviderCount} providers for {_optionsDataCollectionMock.Value.LearnerCount} learners with {_optionsDataCollectionMock.Value.LearningDeliveryCount} learning deliveries from source(s) {_optionsDataCollectionMock.Value.AcademicYear}";
            _logger.LogInformation($"Epao RefreshIlrsEnqueueProviders DataCollectionMockApiClient {generatingInfo}");
        }

        public string BaseAddress()
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetAcademicYears(DateTime dateTimeUtc)
        {
            return await Task.FromResult(new List<string>() { _optionsDataCollectionMock.Value.AcademicYear });
        }

        public async Task<DataCollectionLearnersPage> GetLearners(string source, DateTime startDateTime, int? aimType, int? standardCode, List<int> fundModels, int? progType, int? pageSize, int? pageNumber)
        {
            var learners = _learnerMockDataList;

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
            
            return await Task.FromResult(page);
        }

        public async Task<DataCollectionLearnersPage> GetLearners(string source, int ukprn, int? aimType, int? standardCode, List<int> fundModels, int? progType, int? pageSize, int? pageNumber)
        {
            _learnerMockDataDictionary.TryGetValue(ukprn, out List<DataCollectionLearner> learners);
            if (learners == null)
            {
                learners = _learnerMockDataDictionary[ukprn] = _generator.
                    GetLearners(ukprn, _optionsDataCollectionMock.Value.LearnerCount, _optionsDataCollectionMock.Value.LearningDeliveryCount)
                    .ToList();

                lock (_learnerMockDataList)
                {
                    _learnerMockDataList.AddRange(learners);
                }
            }

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

            return await Task.FromResult(page);
        }

        public async Task<DataCollectionProvidersPage> GetProviders(string source, DateTime startDateTime, int? pageSize, int? pageNumber)
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

            return await Task.FromResult(page);
        }
    }
}
