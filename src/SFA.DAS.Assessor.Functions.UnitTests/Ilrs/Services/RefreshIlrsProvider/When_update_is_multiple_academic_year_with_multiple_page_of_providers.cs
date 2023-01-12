using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsProvider
{
    public class When_update_is_multiple_academic_year_with_multiple_page_of_providers : RefreshIlrsProviderTestBase
    {
        private TestFixture _testFixture;

        public void Arrange()
        {
            _testFixture = new TestFixture()
                .WithProviders("1920", BaseDate.AddDays(-7), new List<int> { 111111, 222222 }, 3)
                .WithProviders("1920", BaseDate, new List<int> { 333333, 444444, 555555, 666666, 777777 }, 3)
                .WithProviders("2021", BaseDate, new List<int> { 555555, 666666, 777777, 888888, 999999 }, 3)
                .WithProviders("1920", BaseDate.AddDays(7), new List<int> { 888888, 999999 }, 3)
                .WithAcademicYear((BaseDate.AddDays(-7), new List<string> { "1920" }))
                .WithAcademicYear((BaseDate, new List<string> { "1920", "2021" }))
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

        [Test]
        public async Task Then_when_exception_is_thrown_log_error_is_written()
        {
            Arrange();

            // Act
            var providerMessages = await _testFixture.ProcessProviders(BaseDate.AddDays(15), BaseDate.AddDays(22));

            // Assert
            providerMessages.Should().BeNull();
            _testFixture.VerifyLogError($"Unable to process providers between {BaseDate.AddDays(15)} and {BaseDate.AddDays(22)}");

        }

        private static DateTime BaseDate = new DateTime(2020, 9, 8);

        private static object[] QueueProviderCases =
        {
            new object[] { BaseDate, 333333, "1920" },
            new object[] { BaseDate, 444444, "1920" },
            new object[] { BaseDate, 555555, "1920" },
            new object[] { BaseDate, 666666, "1920" },
            new object[] { BaseDate, 777777, "1920" },
            new object[] { BaseDate, 555555, "2021" },
            new object[] { BaseDate, 666666, "2021" },
            new object[] { BaseDate, 777777, "2021" },
            new object[] { BaseDate, 888888, "2021" },
            new object[] { BaseDate, 999999, "2021" }
        };
    }
}
