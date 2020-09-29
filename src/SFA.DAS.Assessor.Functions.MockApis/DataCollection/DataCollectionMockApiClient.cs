using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection
{
    public class DataCollectionMockApiClient : IDataCollectionServiceApiClient
    {
        private Dictionary<DateTime, List<DataCollectionLearner>> _learnerMockData = new Dictionary<DateTime, List<DataCollectionLearner>>();
        private Dictionary<DateTime, List<int>> _providerMockData = new Dictionary<DateTime, List<int>>();

        public DataCollectionMockApiClient()
        {
            _learnerMockData.Add(DateTime.Now.Date, new List<DataCollectionLearner>()
            {
                new DataCollectionLearner()
                {
                    DateOfBirth = DateTime.Now.AddYears(-18),
                    GivenNames = "John",
                    FamilyName = "Smith",
                    LearnRefNumber = "888777666",
                    NiNumber = "NZ8288282F",
                    Uln = 444555666,
                    Ukprn = 111222333,
                    LearningDeliveries = new List<DataCollectionLearningDelivery>()
                    {
                        new DataCollectionLearningDelivery()
                        {
                            AchDate = DateTime.Now.AddDays(-60),
                            AimType = 1,
                            CompStatus = 1,
                            EpaOrgID = "EPA0200",
                            FundModel = 36,
                            DelLocPostCode = "TF17KY",
                            LearnActEndDate = DateTime.Now.AddDays(-10),
                            LearnPlanEndDate = DateTime.Now.AddDays(-10),
                            LearnStartDate = DateTime.Now.AddDays(-20),
                            Outcome = 1,
                            OutGrade = "Pass",
                            StdCode = 287,
                            WithdrawReason = 0
                        }
                    }
                }
            });

            _providerMockData.Add(DateTime.Now.Date, new List<int>
            {
                111222333
            });
        }

        public string BaseAddress()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAcademicYears(DateTime dateTimeUtc)
        {
            return Task.FromResult(new List<string>() { "1920" });
        }

        public Task<DataCollectionLearnersPage> GetLearners(string source, DateTime startDateTime, int? aimType, int? standardCode, List<int> fundModels, int? pageSize, int? pageNumber)
        {
            var learners = _learnerMockData[DateTime.Now.Date];

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
            var learners = _learnerMockData[DateTime.Now.Date]
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
            var providers = startDateTime.Date <= DateTime.Now.Date
                ? _providerMockData[startDateTime.Date]
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
