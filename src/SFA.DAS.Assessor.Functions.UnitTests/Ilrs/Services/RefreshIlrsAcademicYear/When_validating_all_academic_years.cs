using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsAcademicYear
{
    public class When_validating_all_academic_years
    {
        TestFixture Fixture;

        private static DateTime Period4Date1920 = new DateTime(2019, 11, 1);
        private static DateTime Period5Date1920 = new DateTime(2019, 12, 1);
        private static DateTime Period6Date1920 = new DateTime(2020, 1, 1);
        private static DateTime Period12Date1920 = new DateTime(2020, 7, 1);
        private static DateTime Period13Date1920Period1Date2021 = new DateTime(2020, 8, 8);
        private static DateTime Period14Date1920Period2Date2021 = new DateTime(2020, 9, 1);

        private static DateTime Period4Date2021 = new DateTime(2020, 11, 1);
        private static DateTime Period5Date2021 = new DateTime(2020, 12, 1);
        private static DateTime Period6Date2021 = new DateTime(2021, 1, 1);
        private static DateTime Period12Date2021 = new DateTime(2021, 7, 1);
        private static DateTime Period13Date2021Period1Date2022 = new DateTime(2021, 8, 8);
        private static DateTime Period14Date2021Period2Date2022 = new DateTime(2021, 9, 1);

        [SetUp]
        public void Arrange()
        {
            Fixture = new TestFixture()
                .WithAcademicYear(Period4Date1920, new List<string> { "1920" })
                .WithAcademicYear(Period5Date1920, new List<string> { "1920" })
                .WithAcademicYear(Period6Date1920, new List<string> { "1920" })
                .WithAcademicYear(Period12Date1920, new List<string> { "1920" })
                .WithAcademicYear(Period13Date1920Period1Date2021, new List<string> { "1920", "2021" })
                .WithAcademicYear(Period14Date1920Period2Date2021, new List<string> { "1920", "2021" })
                .WithAcademicYear(Period4Date2021, new List<string> { "2021" })
                .WithAcademicYear(Period5Date2021, new List<string> { "2021" })
                .WithAcademicYear(Period6Date2021, new List<string> { "2021" })
                .WithAcademicYear(Period12Date2021, new List<string> { "2021" })
                .WithAcademicYear(Period13Date2021Period1Date2022, new List<string> { "2021", "2022" })
                .WithAcademicYear(Period14Date2021Period2Date2022, new List<string> { "2021", "2022" })
                .Setup();

        }

        [TestCaseSource(nameof(ValidateAcademicYearCases))]
        public async Task Then_sources_of_academic_year_are_retrieved(DateTime lastRunDateTime, DateTime currentRunDateTime)
        {
            // Act
            await Fixture.ValidateAllAcademicYears(lastRunDateTime, currentRunDateTime);

            // Assert
            Fixture.DataCollectionServiceApiClient.Verify(v => v.GetAcademicYears(lastRunDateTime), Times.Exactly(1));
            Fixture.DataCollectionServiceApiClient.Verify(v => v.GetAcademicYears(currentRunDateTime), Times.Exactly(1));
        }


        [TestCaseSource(nameof(ValidateAcademicYearCases))]
        public async Task Then_each_academic_year_is_validated(DateTime lastRunDateTime, DateTime currentRunDateTime)
        {
            // Act
            await Fixture.ValidateAllAcademicYears(lastRunDateTime, currentRunDateTime);

            // Assert
            Fixture.DataCollectionServiceApiClient.Verify(v => v.GetProviders(GetAcademicYear(lastRunDateTime), DateTime.MaxValue, 1, 1), Times.Once);
            Fixture.DataCollectionServiceApiClient.Verify(v => v.GetProviders(GetAcademicYear(currentRunDateTime), DateTime.MaxValue, 1, 1), Times.Once);
        }

        private string GetAcademicYear(DateTime date)
        {
            return date.Month > 8 || (date.Month == 8 && date.Day >= 8)
                ? date.ToString("yy") + date.AddYears(1).ToString("yy")
                : date.AddYears(-1).ToString("yy") + date.ToString("yy");
        }

        static object[] ValidateAcademicYearCases =
        {
            new object[] { Period4Date1920, Period5Date1920 },
            new object[] { Period5Date1920, Period6Date1920 },
            new object[] { Period12Date1920, Period13Date1920Period1Date2021 },
            new object[] { Period13Date1920Period1Date2021, Period14Date1920Period2Date2021 },
            new object[] { Period14Date1920Period2Date2021, Period4Date2021 },
        };

        protected class TestFixture
        {
            protected RefreshIlrsAcademicYearService Sut;

            public Mock<IOptions<RefreshIlrsOptions>> Options;
            public Mock<IDataCollectionServiceApiClient> DataCollectionServiceApiClient;
            public Mock<ILogger<RefreshIlrsAcademicYearService>> Logger;

            protected Dictionary<DateTime, List<string>> AcademicYears =
                new Dictionary<DateTime, List<string>>();

            public TestFixture WithAcademicYear(DateTime date, List<string> academicYears)
            {
                AcademicYears
                    .Add(date, academicYears);

                return this;
            }

            public TestFixture Setup()
            {
                Options = new Mock<IOptions<RefreshIlrsOptions>>();
                Options.Setup(p => p.Value).Returns(new RefreshIlrsOptions
                {
                    ProviderPageSize = 1,
                    ProviderInitialRunDate = new DateTime(2019, 10, 10),
                    LearnerPageSize = 1,
                    LearnerFundModels = "10, 20, 30"
                });

                DataCollectionServiceApiClient = new Mock<IDataCollectionServiceApiClient>();
                DataCollectionServiceApiClient.Setup(v => v.GetAcademicYears(It.Is<DateTime>(p => AcademicYears.ContainsKey(p)))).ReturnsAsync((DateTime period) => AcademicYears[period]);
                DataCollectionServiceApiClient.Setup(v => v.GetProviders(It.Is<string>(p => p == "1920" || p == "2021"), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>()))
                    .ReturnsAsync(new DataCollectionProvidersPage());

                Logger = new Mock<ILogger<RefreshIlrsAcademicYearService>>();

                Sut = new RefreshIlrsAcademicYearService(
                    Options.Object,
                    DataCollectionServiceApiClient.Object,
                    Logger.Object);

                return this;
            }

            public async Task<List<string>> ValidateAllAcademicYears(DateTime lastRunDateTime, DateTime currentRunDateTime)
            {
                return await Sut.ValidateAllAcademicYears(lastRunDateTime, currentRunDateTime);
            }
        }
    }
}
