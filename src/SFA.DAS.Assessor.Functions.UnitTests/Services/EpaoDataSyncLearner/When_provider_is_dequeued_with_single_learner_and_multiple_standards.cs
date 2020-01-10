using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Config;
using SFA.DAS.Assessor.Functions.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Services.EpaoDataSyncLearner
{
    public class When_provider_is_dequeued_with_single_learner_and_multiple_standards : EpaoDataSyncLearnerTestBase
    {
        [SetUp]
        public void Arrange()
        {
            BaseArrange();
        }

        [TestCase(1)]
        public async Task Then_learner_details_are_retrieved(int pageNumber)
        {
            // Arrange
            var providerMessage = new EpaoDataSyncProviderMessage
            {
                Source = "1920",
                Ukprn = UkprnFour,
                LearnerPageNumber = pageNumber
            };


            // Act
            await Sut.ProcessLearners(providerMessage);

            // Assert            
            var optionsLearnerFundModels = ConfigHelper.ConvertCsvValueToList<int>(Options.Object.Value.LearnerFundModels);
            DataCollectionServiceApiClient.Verify(p => p.GetLearners(
                "1920",
                UkprnFour,
                1,
                -1,
                It.Is<List<int>>(p => Enumerable.SequenceEqual(p, optionsLearnerFundModels)),
                Options.Object.Value.LearnerPageSize,
                pageNumber), Times.Once);
        }

        [Test]
        public async Task Then_learner_details_import_request_is_sent_to_assessor()
        {
            // Arrange
            var providerMessage = new EpaoDataSyncProviderMessage
            {
                Source = "1920",
                Ukprn = UkprnFour,
                LearnerPageNumber = 1
            };

            // Act
            await Sut.ProcessLearners(providerMessage);

            // Assert       
            AssertLearnerDetailRequest(UkprnFourOne);
        }
    }
}
