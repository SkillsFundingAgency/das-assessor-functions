using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Services.EpaoDataSyncProvider
{
    public class When_update_is_single_academic_year_with_single_page_of_providers : EpaoDataSyncProviderTestBase
    {
        [SetUp]
        public void Arrange()
        {
            BaseArrange();
            ArrangeEpaoDataSyncLastRunDate(Period4Date1920.ToString("o"));
        }

        [Test]
        public async Task Then_the_last_run_date_is_obtained()
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            AssessorServiceApiClient.Verify(p => p.GetAssessorSetting("EpaoDataSyncLastRunDate"), Times.Once);
        }


        [Test]
        public async Task Then_sources_of_academic_year_are_retrieved_for_last_run_date()
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            DataCollectionServiceApiClient.Verify(p => p.GetAcademicYears(Period4Date1920), Times.Once);
        }


        [TestCase("1920")]
        public async Task Then_each_academic_year_is_validated(string source)
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            DataCollectionServiceApiClient.Verify(p => p.GetProviders(source, DateTime.MaxValue, 1, 1), Times.Once);
        }


        [TestCase(111111, "1920")]
        [TestCase(222222, "1920")]
        [TestCase(333333, "1920")]
        public async Task Then_each_updated_provider_is_queued(int ukprn, string source)
        {
            // Act
            await Sut.ProcessProviders();

            // Assert
            StorageQueueService.Verify(p => p.SerializeAndQueueMessage(It.Is<EpaoDataSyncProviderMessage>(p => p.Ukprn == ukprn && p.Source == source)), Times.Once);
        }
    }
}
