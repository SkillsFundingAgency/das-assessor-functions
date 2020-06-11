﻿using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Config;
using SFA.DAS.Assessor.Functions.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.Assessor.Functions.UnitTests.Services.EpaoDataSyncLearner
{
    public class When_provider_is_dequeued_with_multiple_learners_and_multiple_standards_and_multiple_fundmodels : EpaoDataSyncLearnerTestBase
    {
        [SetUp]
        public void Arrange()
        {
            BaseArrange();
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task Then_learner_details_are_retrieved(int pageNumber)
        {
            // Arrange
            var providerMessage = new EpaoDataSyncProviderMessage
            {
                Source = "1920",
                Ukprn = UkprnTwo,
                LearnerPageNumber = pageNumber
            };


            // Act
            await Sut.ProcessLearners(providerMessage);

            // Assert            
            var optionsLearnerFundModels = ConfigHelper.ConvertCsvValueToList<int>(Options.Object.Value.LearnerFundModels);
            DataCollectionServiceApiClient.Verify(
                v => v.GetLearners(
                    "1920",
                    UkprnTwo,
                    1,
                    -1,
                    It.Is<List<int>>(p => Enumerable.SequenceEqual(p, optionsLearnerFundModels)),
                    Options.Object.Value.LearnerPageSize,
                    pageNumber), 
                Times.Once);
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        public async Task Then_subsequent_page_provider_is_queued(int pageNumber, bool subsequentPageQueued)
        {
            // Arrange
            var providerMessage = new EpaoDataSyncProviderMessage
            {
                Source = "1920",
                Ukprn = UkprnFour,
                LearnerPageNumber = pageNumber
            };

            // Act
            var nextPageProviderMessage = await Sut.ProcessLearners(providerMessage);

            // Assert
            nextPageProviderMessage.Should().Match(p => subsequentPageQueued && p != null || !subsequentPageQueued && p == null);
        }

        [Test]
        public async Task Then_learner_details_import_request_is_sent_to_assessor()
        {
            // Arrange
            var providerMessage = new EpaoDataSyncProviderMessage
            {
                Source = "1920",
                Ukprn = UkprnTwo,
                LearnerPageNumber = 1
            };

            // Act
            await Sut.ProcessLearners(providerMessage);

            // Assert       
            AssertLearnerDetailRequest(UkprnTwoOne);
            AssertLearnerDetailRequest(UkprnTwoTwo);
        }
    }
}