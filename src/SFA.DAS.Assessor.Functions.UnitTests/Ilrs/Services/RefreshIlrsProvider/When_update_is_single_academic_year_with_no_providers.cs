using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsProvider
{
    public class When_update_is_single_academic_year_with_no_providers : RefreshIlrsProviderTestBase
    {
        private TestFixture _testFixture;

        public void Arrange()
        {
            _testFixture = new TestFixture()
                .WithProviders("1920", BaseDate.AddDays(-7), new List<int> { 111111, 222222 }, 3)
                .WithProviders("1920", BaseDate, new List<int> { }, 3)
                .WithProviders("1920", BaseDate.AddDays(7), new List<int> { 888888, 999999 }, 3)
                .WithAcademicYear((BaseDate.AddDays(-7), new List<string> { "1920" }))
                .WithAcademicYear((BaseDate, new List<string> { "1920" }))
                .WithAcademicYear((BaseDate.AddDays(7), new List<string> { "1920" }))
                .Setup();
        }

        [Test]
        public async Task Then_sources_of_academic_year_are_retrieved()
        {
            Arrange();

            // Act
            await _testFixture.ProcessProviders(BaseDate, BaseDate.AddDays(7));

            // Assert
            _testFixture.MockRefreshIlrsAcademicYearsService.Verify(v => v.ValidateAllAcademicYears(BaseDate, BaseDate.AddDays(7)), Times.Once);
        }

        [Test]
        public async Task Then_each_updated_provider_is_queued()
        {
            Arrange();

            // Act
            var providerMessages = await _testFixture.ProcessProviders(BaseDate, BaseDate.AddDays(7));

            // Assert
            providerMessages.Should().BeEmpty();
        }

        private static DateTime BaseDate = new DateTime(2020, 2, 8);
    }
}
