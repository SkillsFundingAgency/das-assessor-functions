using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.UnitTests.Services.EpaoDataSyncProvider
{
    public class EpaoDataSyncProviderTestBase
    {
        protected EpaoDataSyncProviderService Sut;

        protected Mock<IOptions<EpaoDataSync>> Options;
        protected Mock<IDataCollectionServiceApiClient> DataCollectionServiceApiClient;
        protected Mock<IAssessorServiceApiClient> AssessorServiceApiClient;
        protected Mock<IDateTimeHelper> DateTimeHelper;
        protected Mock<ILogger<EpaoDataSyncProviderService>> Logger;

        protected string EpaoDataSyncLastRunDate = null;

        protected static DateTime Period4Date1920 = new DateTime(2019, 11, 1);
        protected static DateTime Period5Date1920 = new DateTime(2019, 12, 1);
        protected static DateTime Period6Date1920 = new DateTime(2020, 1, 1);
        protected static DateTime Period13Date1920Period1Date2021 = new DateTime(2020, 8, 1);
        protected static DateTime Period14Date1920Period2Date2021 = new DateTime(2020, 9, 1);

        protected static DateTime Period4Date2021 = new DateTime(2020, 11, 1);
        protected static DateTime Period5Date2021 = new DateTime(2020, 12, 1);
        protected static DateTime Period6Date2021 = new DateTime(2021, 1, 1);
        protected static DateTime Period13Date2021Period1Date2022 = new DateTime(2021, 8, 1);
        protected static DateTime Period14Date2021Period2Date2022 = new DateTime(2021, 9, 1);

        protected Dictionary<DateTime, List<string>> AcademicYears = new Dictionary<DateTime, List<string>>
        {
            {
              Period4Date1920, new List<string> { "1920" }
            },
            {
              Period5Date1920, new List<string> { "1920" }
            },
            {
              Period6Date1920, new List<string> { "1920" }
            },
            {
              Period13Date1920Period1Date2021, new List<string> { "1920", "2021" }
            },
            {
              Period14Date1920Period2Date2021, new List<string> { "1920", "2021" }
            },
            {
              Period4Date2021, new List<string> { "2021" }
            },
            {
              Period5Date2021, new List<string> { "2021" }
            },
            {
              Period6Date2021, new List<string> { "2021" }
            },
            {
              Period13Date2021Period1Date2022, new List<string> { "2021", "2022" }
            },
            {
              Period14Date2021Period2Date2022, new List<string> { "2021", "2022" }
            },
        };

        protected Dictionary<(DateTime, int), DataCollectionProvidersPage> Providers1920 = new Dictionary<(DateTime, int), DataCollectionProvidersPage>
        {
            {
                (DateTime.MaxValue, 1 ), new DataCollectionProvidersPage()
            },
            {
                ( Period4Date1920, 1 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 3, TotalPages = 1 },
                    Providers = new List<int> { 111111, 222222, 333333 }
                }
            },
            {
                ( Period4Date1920, 2 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 3, TotalPages = 1 },
                    Providers = new List<int> {  }
                }
            },
            {
                ( Period6Date1920, 1 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 6, TotalPages = 1 },
                    Providers = new List<int> { 111111, 222222, 333333 }
                }
            },
            {
                ( Period6Date1920, 2 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 6 , TotalPages = 2 },
                    Providers = new List<int> { 444444, 555555, 666666 }
                }
            },
            {
                ( Period6Date1920, 3 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 3, PageSize = 3, TotalItems = 6, TotalPages = 2 },
                    Providers = new List<int> {  }
                }
            },
            {
                ( Period13Date1920Period1Date2021, 1 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 3, TotalPages = 1 },
                    Providers = new List<int> { 444444, 555555, 666666 }
                }
            },
            {
                ( Period13Date1920Period1Date2021, 2 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 3, TotalPages = 1 },
                    Providers = new List<int> {  }
                }
            },
            {
                ( Period14Date1920Period2Date2021, 1 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 5, TotalPages = 2 },
                    Providers = new List<int> { 777777, 888888, 999999 }
                }
            },
            {
                ( Period14Date1920Period2Date2021, 2 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 5, TotalPages = 2 },
                    Providers = new List<int> { 1111111, 1222222 }
                }
            },
            {
                ( Period14Date1920Period2Date2021, 3 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 3, PageSize = 3, TotalItems = 5, TotalPages = 2 },
                    Providers = new List<int> {  }
                }
            }
        };

        protected Dictionary<(DateTime, int), DataCollectionProvidersPage> Providers2021 = new Dictionary<(DateTime, int), DataCollectionProvidersPage>
        {
            {
                (DateTime.MaxValue, 1), new DataCollectionProvidersPage()
            },
            {
                (Period13Date1920Period1Date2021, 1), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 3, TotalPages = 1 },
                    Providers = new List<int> { 444444, 555555, 777777 }
                }
            },
            {
                (Period13Date1920Period1Date2021, 2), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 3, TotalPages = 1 },
                    Providers = new List<int> {  }
                }
            },
            {
                ( Period14Date1920Period2Date2021, 1 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 1, PageSize = 3, TotalItems = 5, TotalPages = 2 },
                    Providers = new List<int> { 777777, 1333333, 1444444 }
                }
            },
            {
                ( Period14Date1920Period2Date2021, 2 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 2, PageSize = 3, TotalItems = 5, TotalPages = 2 },
                    Providers = new List<int> { 1555555, 1666666 }
                }
            },
            {
                ( Period14Date1920Period2Date2021, 3 ), new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo {PageNumber = 3, PageSize = 3, TotalItems = 5, TotalPages = 2 },
                    Providers = new List<int> {  }
                }
            }
        };

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
            DataCollectionServiceApiClient.Setup(v => v.GetAcademicYears(It.Is<DateTime>(p => AcademicYears.ContainsKey(p)))).ReturnsAsync((DateTime period) => AcademicYears[period]);
            DataCollectionServiceApiClient.Setup(v => v.GetProviders("1920", It.Is<DateTime>(p => Providers1920.ContainsKey(new Tuple<DateTime, int>(p, 1).ToValueTuple())), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((string source, DateTime period, int? pageSize, int? pageNumber) => Providers1920[(period, pageNumber.Value)]);
            DataCollectionServiceApiClient.Setup(v => v.GetProviders("2021", It.Is<DateTime>(p => Providers2021.ContainsKey(new Tuple<DateTime, int>(p, 1).ToValueTuple())), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((string source, DateTime period, int? pageSize, int? pageNumber) => Providers2021[(period, pageNumber.Value)]);

            AssessorServiceApiClient = new Mock<IAssessorServiceApiClient>();
            ArrangeEpaoDataSyncLastRunDate(EpaoDataSyncLastRunDate);

            DateTimeHelper = new Mock<IDateTimeHelper>();
            Logger = new Mock<ILogger<EpaoDataSyncProviderService>>();

            Sut = new EpaoDataSyncProviderService(Options.Object, DataCollectionServiceApiClient.Object, AssessorServiceApiClient.Object, Logger.Object);
        }

        protected void ArrangeEpaoDataSyncLastRunDate(string lastRunDateTime)
        {
            if (AssessorServiceApiClient != null)
            {
                AssessorServiceApiClient.Setup(p => p.GetAssessorSetting("EpaoDataSyncLastRunDate")).ReturnsAsync(lastRunDateTime);
            }
        }
    }
}
