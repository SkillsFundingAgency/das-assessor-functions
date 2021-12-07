using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsLearner
{
    public class When_provider_is_dequeued_with_single_learner_and_multiple_standards : RefreshIlrsLearnerTestBase
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
            var providerMessage = new RefreshIlrsProviderMessage
            {
                Source = "1920",
                Ukprn = UkprnFour,
                LearnerPageNumber = pageNumber
            };


            // Act
            await Sut.ProcessLearners(providerMessage);

            // Assert
            var optionsLearnerFundModels = ConfigurationHelper.ConvertCsvValueToList<int>(Options.Object.Value.LearnerFundModels);
            DataCollectionServiceApiClient.Verify(
                v => v.GetLearners(
                    "1920",
                    UkprnFour,
                    1,
                    -1,
                    It.Is<List<int>>(p => Enumerable.SequenceEqual(p, optionsLearnerFundModels)),
                    -1,
                    Options.Object.Value.LearnerPageSize,
                    pageNumber), 
                Times.Once);
        }

        [Test]
        public async Task Then_learner_details_import_request_is_sent_to_assessor()
        {
            // Arrange
            var providerMessage = new RefreshIlrsProviderMessage
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
