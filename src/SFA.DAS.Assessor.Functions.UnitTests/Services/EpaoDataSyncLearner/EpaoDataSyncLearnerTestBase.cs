using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Assessor.Functions.Config;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.UnitTests.Services.EpaoDataSyncLearner
{
    public class EpaoDataSyncLearnerTestBase
    {
        protected EpaoDataSyncLearnerService Sut;

        protected Mock<IOptions<EpaoDataSync>> Options;
        protected Mock<IDataCollectionServiceApiClient> DataCollectionServiceApiClient;
        protected Mock<IAssessorServiceApiClient> AssessorServiceApiClient;
        protected Mock<IEpaoServiceBusQueueService> EpaoServiceBusQueueService;
        protected Mock<ILogger<EpaoDataSyncLearnerService>> Logger;

        protected static int UkprnOne = 111111;
        protected static int UkprnTwo = 222222;
        protected static int UkprnThree = 333333;
        protected static int UkprnFour = 444444;

        protected static LearnerTestData UkprnOneOne = new LearnerTestData { Ukprn = UkprnOne, Uln = 1, StdCodes = new List<int?> { 80 }, FundModels = new List<int> { 20 } };
        protected static LearnerTestData UkprnTwoOne = new LearnerTestData { Ukprn = UkprnTwo, Uln = 1, StdCodes = new List<int?> { 80, 100 }, FundModels = new List<int> { 20 } };
        protected static LearnerTestData UkprnTwoTwo = new LearnerTestData { Ukprn = UkprnTwo, Uln = 2, StdCodes = new List<int?> { 80, 200 }, FundModels = new List<int> { 20 } };
        protected static LearnerTestData UkprnThreeOne = new LearnerTestData { Ukprn = UkprnThree, Uln = 1, StdCodes = new List<int?> { 80, 200, null }, FundModels = new List<int> { 20, 30, 35 } };
        protected static LearnerTestData UkprnFourOne = new LearnerTestData { Ukprn = UkprnFour, Uln = 1, StdCodes = new List<int?> { null, null }, FundModels = new List<int> { 5, 10, 20 } };
        protected static LearnerTestData UkprnFourTwo = new LearnerTestData { Ukprn = UkprnFour, Uln = 1, StdCodes = new List<int?> { 60, 70 }, FundModels = new List<int> { 10 } };

        protected Dictionary<(int, int), DataCollectionLearnersPage> Learners1920 = new Dictionary<(int, int), DataCollectionLearnersPage>
        {
            {
                (UkprnOne, 1), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>
                    {
                        GenerateTestLearner(UkprnOneOne.Ukprn, UkprnOneOne.Uln, UkprnOneOne.StdCodes, UkprnOneOne.FundModels),                       
                    }
                }
            },
            {
                (UkprnOne, 2), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>()
                }
            },
            {
                (UkprnTwo, 1), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>
                    {
                        GenerateTestLearner(UkprnTwoOne.Ukprn, UkprnTwoOne.Uln, UkprnTwoOne.StdCodes, UkprnTwoOne.FundModels),
                        GenerateTestLearner(UkprnTwoTwo.Ukprn, UkprnTwoTwo.Uln, UkprnTwoTwo.StdCodes, UkprnTwoTwo.FundModels)
                    }
                }
            },
            {
                (UkprnTwo, 2), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>()
                }
            },
            {
                (UkprnThree, 1), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>
                    {
                        GenerateTestLearner(UkprnThree, UkprnThreeOne.Uln, UkprnThreeOne.StdCodes, UkprnThreeOne.FundModels),
                    }
                }
            },
            {
                (UkprnThree, 2), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>()
                }
            },
            {
                (UkprnFour, 1), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 1, TotalItems = 2, TotalPages = 2 },
                    Learners = new List<DataCollectionLearner>
                    {
                        GenerateTestLearner(UkprnFour, UkprnFourOne.Uln, UkprnFourOne.StdCodes, UkprnFourOne.FundModels),
                    }
                }
            },
            {
                (UkprnFour, 2), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 1, TotalItems = 2, TotalPages = 2 },
                    Learners = new List<DataCollectionLearner>
                    {
                        GenerateTestLearner(UkprnFour, UkprnFourTwo.Uln, UkprnFourTwo.StdCodes, UkprnFourTwo.FundModels),
                    }
                }
            },
            {
                (UkprnFour, 3), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 3, PageSize = 1, TotalItems = 2, TotalPages = 2 },
                    Learners = new List<DataCollectionLearner>()
                }
            }
        };

        protected static DataCollectionLearner GenerateTestLearner(int ukprn, int uln, List<int?> stdCodes, List<int> fundModels)
        {
            var learner = new DataCollectionLearner
            {
                Ukprn = ukprn,
                Uln = GenerateUln(ukprn, uln),
                GivenNames = "BLANK",
                FamilyName = "BLANK",
                LearnRefNumber = "BLANK",
            };
                
            foreach(var stdCode in stdCodes)
            {
                foreach(var fundModel in fundModels)
                {
                    learner.LearningDeliveries.Add(new DataCollectionLearningDelivery
                    {
                        AimType = 1,
                        StdCode = stdCode,
                        FundModel = fundModel,
                        EpaOrgID = "BLANK",
                        LearnStartDate = DateTime.Now.AddDays(1),
                        LearnPlanEndDate = DateTime.Now.AddDays(50),
                        CompStatus = null,
                        DelLocPostCode = "BBLANK1",
                        LearnActEndDate = null,
                        WithdrawReason = null,
                        Outcome = null,
                        AchDate = null,
                        OutGrade = "BLANK"
                    }); ;
                }
            }

            return learner;
        }

        protected static int GenerateUln(int ukprn, int uln)
        {
            return (uln * 1000000) + ukprn;
        }

        protected void BaseArrange()
        {
            Options = new Mock<IOptions<EpaoDataSync>>();
            Options.Setup(p => p.Value).Returns(new EpaoDataSync
            {
                ProviderPageSize = 1,
                ProviderInitialRunDate = new DateTime(2019, 10, 10),
                LearnerPageSize = 1,
                LearnerFundModels = "10, 20, 30"
            });

            DataCollectionServiceApiClient = new Mock<IDataCollectionServiceApiClient>();
            DataCollectionServiceApiClient.Setup(p => p.GetLearners("1920", It.Is<int>(p => Learners1920.ContainsKey(new Tuple<int, int>(p, 1).ToValueTuple())), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((string source, int ukprn, int? aimType, int? standardCode, List<int> fundModels,  int? pageSize, int? pageNumber) => Learners1920[(ukprn, pageNumber.Value)]);
           
            AssessorServiceApiClient = new Mock<IAssessorServiceApiClient>();
            EpaoServiceBusQueueService = new Mock<IEpaoServiceBusQueueService>();
            Logger = new Mock<ILogger<EpaoDataSyncLearnerService>>();

            Sut = new EpaoDataSyncLearnerService(Options.Object, DataCollectionServiceApiClient.Object, AssessorServiceApiClient.Object, EpaoServiceBusQueueService.Object, Logger.Object);
        }

        protected void AssertLearnerDetailRequest(LearnerTestData learnerTestData)
        {
            var importedLearners = new List<(int, int)>();
            var ignoredLearners = new List<(int?, int)>();

            var optionsLearnerFundModels = ConfigHelper.ConvertCsvValueToList<int>(Options.Object.Value.LearnerFundModels);
            foreach (var stdCode in learnerTestData.StdCodes)
            {
                if (stdCode != null)
                {
                    foreach (var fundModel in learnerTestData.FundModels)
                    {
                        if (optionsLearnerFundModels.Contains(fundModel))
                        {
                            importedLearners.Add((stdCode.Value, fundModel));
                        }
                        else
                        {
                            ignoredLearners.Add((stdCode, fundModel));
                        }
                    }
                }
                else
                {
                    foreach (var fundModel in learnerTestData.FundModels)
                    {
                        ignoredLearners.Add((stdCode, fundModel));
                    }
                }
            }

            foreach ((int, int) importedLearner in importedLearners)
            {
                AssessorServiceApiClient.Verify(p => p.ImportLearnerDetails(It.Is<ImportLearnerDetailRequest>(
                p => p.ImportLearnerDetails.Exists(p =>
                p.Ukprn == learnerTestData.Ukprn &&
                p.Uln == GenerateUln(learnerTestData.Ukprn, learnerTestData.Uln) &&
                p.StdCode == importedLearner.Item1 &&
                p.FundingModel == importedLearner.Item2))), Times.Once);
            }

            foreach ((int?, int) ignoredLearner in ignoredLearners)
            {
                AssessorServiceApiClient.Verify(p => p.ImportLearnerDetails(It.Is<ImportLearnerDetailRequest>(
                p => p.ImportLearnerDetails.Exists(p =>
                p.Ukprn == learnerTestData.Ukprn &&
                p.Uln == GenerateUln(learnerTestData.Ukprn, learnerTestData.Uln) &&
                p.StdCode == ignoredLearner.Item1 &&
                p.FundingModel == ignoredLearner.Item2))), Times.Never);
            }
        }

        protected class LearnerTestData
        {
            public int Ukprn { get; set; }
            public int Uln { get; set; }
            public List<int?> StdCodes { get; set; }
            public List<int> FundModels { get; set; }
        }
    }
}
