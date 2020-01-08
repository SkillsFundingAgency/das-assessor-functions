using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
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
        protected Mock<ILogger<EpaoDataSyncLearnerService>> Logger;

        protected static int UkprnOne = 111111;

        protected Dictionary<(int, int), DataCollectionLearnersPage> Learners1920 = new Dictionary<(int, int), DataCollectionLearnersPage>
        {
            {
                (UkprnOne, 1), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>
                    {
                        GenerateTestLearner(UkprnOne, 1, new List<int?>{80}, new List<int>{10}),
                        GenerateTestLearner(UkprnOne, 2, new List<int?>{80}, new List<int>{10}),
                    }
                }
            },
            {
                (UkprnOne, 2), new DataCollectionLearnersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 2, TotalPages = 1 },
                    Learners = new List<DataCollectionLearner>()
                }
            }
        };

        protected static DataCollectionLearner GenerateTestLearner(int ukprn, int uln, List<int?> stdCodes, List<int> fundModels)
        {
            var learner = new DataCollectionLearner
            {
                Ukprn = ukprn,
                Uln = (uln * 1000000) + ukprn ,
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
            Logger = new Mock<ILogger<EpaoDataSyncLearnerService>>();

            Sut = new EpaoDataSyncLearnerService(Options.Object, DataCollectionServiceApiClient.Object, AssessorServiceApiClient.Object, Logger.Object);
        }
    }
}
