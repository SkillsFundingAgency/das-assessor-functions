using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.EpaoDataSync.Services.EpaoDataSyncProvider
{
    public class When_update_is_multiple_academic_year_with_multiple_page_of_providers : EpaoDataSyncProviderTestBase
    {
        [SetUp]
        public void Arrange()
        {
            BaseArrange();
            ArrangeEpaoDataSyncLastRunDate(Period14Date1920Period2Date2021.ToString("o"));
        }

        [Test]
        public async Task Then_the_last_run_date_is_obtained()
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            AssessorServiceApiClient.Verify(v => v.GetAssessorSetting("EpaoDataSyncLastRunDate"), Times.Once);
        }


        [Test]
        public async Task Then_sources_of_academic_year_are_retrieved_for_last_run_date()
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            DataCollectionServiceApiClient.Verify(v => v.GetAcademicYears(Period14Date1920Period2Date2021), Times.Once);
        }


        [TestCase("1920")]
        [TestCase("2021")]
        public async Task Then_each_academic_year_is_validated(string source)
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            DataCollectionServiceApiClient.Verify(v => v.GetProviders(source, DateTime.MaxValue, 1, 1), Times.Once);
        }


        [TestCase(777777, "1920")]
        [TestCase(888888, "1920")]
        [TestCase(999999, "1920")]
        [TestCase(1111111, "1920")]
        [TestCase(1222222, "1920")]
        [TestCase(777777, "2021")]
        [TestCase(1333333, "2021")]
        [TestCase(1444444, "2021")]
        [TestCase(1555555, "2021")]
        [TestCase(1666666, "2021")]
        public async Task Then_each_updated_provider_is_queued(int ukprn, string source)
        {
            // Act
            var providerMessages = await Sut.ProcessProviders();

            // Assert
            providerMessages.Should().ContainSingle(p => p.Ukprn == ukprn && p.Source == source);
        }
    }
}
