using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsProvider
{
    public class When_update_is_single_academic_year_with_single_page_of_providers : RefreshIlrsProviderTestBase
    {
        private TestFixture _testFixture;

        public void Arrange()
        {
            _testFixture = new TestFixture()
                .WithProviders("1920", BaseDate.AddDays(-7), new List<int> { 111111, 222222 }, 3)
                .WithProviders("1920", BaseDate, new List<int> { 333333, 444444, 555555, 666666, 777777 }, 3)
                .WithProviders("1920", BaseDate.AddDays(7), new List<int> { 888888, 999999 }, 3)
                .WithAcademicYear((BaseDate.AddDays(-7), new List<string> { "1920" }))
                .WithAcademicYear((BaseDate, new List<string> { "1920" }))
                .WithAcademicYear((BaseDate.AddDays(7), new List<string> { "1920" }))
                .Setup();
        }

        [TestCaseSource(nameof(QueueProviderCases))]
        public async Task Then_sources_of_academic_year_are_retrieved(DateTime runDate, int ukprn, string source)
        {
            Arrange();

            // Act
            var providerMessages = await _testFixture.ProcessProviders(runDate, runDate.AddDays(7));

            // Assert
            _testFixture.MockRefreshIlrsAcademicYearsService.Verify(v => v.ValidateAllAcademicYears(runDate, runDate.AddDays(7)), Times.Once);
        }

        [TestCaseSource(nameof(QueueProviderCases))]
        public async Task Then_each_updated_provider_is_queued(DateTime runDate, int ukprn, string source)
        {
            Arrange();

            // Act
            var providerMessages = await _testFixture.ProcessProviders(runDate, runDate.AddDays(7));

            // Assert
            providerMessages.Should().ContainSingle(p => p.Ukprn == ukprn && p.Source == source);
        }

        private static DateTime BaseDate = new DateTime(2020, 2, 8);

        private static object[] QueueProviderCases =
        {
            new object[] { BaseDate.AddDays(-7), 111111, "1920" },
            new object[] { BaseDate.AddDays(-7), 222222, "1920" }
        };
    }
}
