using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Services.EpaoDataSyncLearner
{
    public class When_provider_is_dequeued_with_single_learner_and_single_standard : EpaoDataSyncLearnerTestBase
    {
        [SetUp]
        public void Arrange()
        {
            BaseArrange();
        }

        [Test]
        public async Task Then_learner_details_are_retrieved()
        {
            // Arrange
            var providerMessage = new EpaoDataSyncProviderMessage
            { 
                Source = "1920",
                Ukprn = UkprnOne
            };


            // Act
            await Sut.ProcessLearners(providerMessage);

            // Assert            
            DataCollectionServiceApiClient.Verify(p => p.GetLearners(
                "1920", 
                UkprnOne, 
                1, 
                -1, 
                It.Is<List<int>>(p => Enumerable.SequenceEqual(p, Options.Object.Value.LearnerFundModelList)), 
                Options.Object.Value.LearnerPageSize, 
                1), Times.Once);

            DataCollectionServiceApiClient.Verify(p => p.GetLearners(
                "1920",
                UkprnOne,
                1,
                -1,
                It.Is<List<int>>(p => Enumerable.SequenceEqual(p, Options.Object.Value.LearnerFundModelList)),
                Options.Object.Value.LearnerPageSize,
                2), Times.Once);
        }
    }
}
