using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsAccessorSetting
{
    public class When_getting_last_run_date
    {
        [Test]
        public async Task Then_accessor_api_called_to_get_last_run_date()
        {
            // Arrange
            var lastRunDate = new DateTime(2020, 01, 01);
            var fixture = new TestFixture()
                .WithRefreshIlrsLastRunDate(lastRunDate)
                .WithOptions(new RefreshIlrsOptions
                {
                    ProviderInitialRunDate = DateTime.MinValue
                })
                .Setup();

            // Act
            await fixture.GetLastRunDateTime();

            // Assert
            fixture.VerifyGetAssessorSettingCalled();
        }

        [Test]
        public async Task Then_refresh_ilrs_last_run_date_returned()
        {
            // Arrange
            var lastRunDate = new DateTime(2020, 01, 01);
            var fixture = new TestFixture()
                .WithRefreshIlrsLastRunDate(lastRunDate)
                .WithOptions(new RefreshIlrsOptions
                {
                    ProviderInitialRunDate = DateTime.MinValue
                })
                .Setup();

            // Act
            var result = await fixture.GetLastRunDateTime();

            // Assert
            result.Should().Be(lastRunDate);
        }

        [Test]
        public async Task Then_no_refresh_ilrs_last_run_date_returns_default()
        {
            // Arrange
            var providerInitialRunDate = new DateTime(2020, 01, 01);
            var fixture = new TestFixture()
                .WithRefreshIlrsLastRunDate(null)
                .WithOptions(new RefreshIlrsOptions
                {
                    ProviderInitialRunDate = providerInitialRunDate
                })
                .Setup();

            // Act
            var result = await fixture.GetLastRunDateTime();

            // Assert
            result.Should().Be(providerInitialRunDate);
        }

        private class TestFixture
        {
            protected RefreshIlrsAccessorSettingService Sut;

            public Mock<IOptions<RefreshIlrsOptions>> Options;
            public Mock<IAssessorServiceApiClient> AssessorServiceApiClient;
            public Mock<ILogger<RefreshIlrsAccessorSettingService>> Logger;

            public TestFixture()
            {
                Options = new Mock<IOptions<RefreshIlrsOptions>>();
                AssessorServiceApiClient = new Mock<IAssessorServiceApiClient>();
                Logger = new Mock<ILogger<RefreshIlrsAccessorSettingService>>();
            }

            public TestFixture Setup()
            {
                Sut = new RefreshIlrsAccessorSettingService(
                    Options.Object,
                    AssessorServiceApiClient.Object,
                    Logger.Object);

                return this;
            }

            public async Task<DateTime> GetLastRunDateTime()
            {
                return await Sut.GetLastRunDateTime();
            }

            public TestFixture WithRefreshIlrsLastRunDate(DateTime? refreshIlrsLastRunDate)
            {
                AssessorServiceApiClient.Setup(p => p.GetAssessorSetting("RefreshIlrsLastRunDate"))
                    .ReturnsAsync(refreshIlrsLastRunDate?.ToString("o"));
                
                return this;
            }

            public TestFixture WithOptions(RefreshIlrsOptions options)
            {
                Options.Setup(p => p.Value).Returns(options);
                
                return this;
            }

            public void VerifyGetAssessorSettingCalled()
            {
                AssessorServiceApiClient.Verify(p => p.GetAssessorSetting("RefreshIlrsLastRunDate"), Times.Once);
            }
        }
    }
}
