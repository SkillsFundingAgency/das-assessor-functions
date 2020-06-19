using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.EpaoDataSync.Services.EpaoDataSyncProvider
{
    public class When_update_is_multiple_academic_year_with_single_page_of_providers : EpaoDataSyncProviderTestBase
    {
        [SetUp]
        public void Arrange()
        {
            BaseArrange(Period13Date1920Period1Date2021.ToString("o"));
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
        public async Task Then_sources_of_academic_year_are_retrieved()
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            DataCollectionServiceApiClient.Verify(v => v.GetAcademicYears(Period13Date1920Period1Date2021), Times.Exactly(2));
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


        [TestCase(444444, "1920")]
        [TestCase(555555, "1920")]
        [TestCase(666666, "1920")]
        [TestCase(444444, "2021")]
        [TestCase(555555, "2021")]
        [TestCase(777777, "2021")]
        public async Task Then_each_updated_provider_is_queued(int ukprn, string source)
        {
            // Act
            var providerMessages = await Sut.ProcessProviders();

            // Assert
            providerMessages.Should().ContainSingle(p => p.Ukprn == ukprn && p.Source == source);
        }
    }
}
