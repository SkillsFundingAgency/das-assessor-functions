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
    public class When_setting_last_run_date
    {
        [Test]
        public async Task Then_accessor_api_called_to_set_last_run_date()
        {
            // Arrange
            var nextRunDateTime = new DateTime(2021, 12, 31);
            var fixture = new TestFixture()
                .Setup();

            // Act
            await fixture.SetLastRunDateTime(nextRunDateTime);

            // Assert
            fixture.VerifySetAssessorSettingCalled(nextRunDateTime);
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

            public async Task SetLastRunDateTime(DateTime nextRunDateTime)
            {
                await Sut.SetLastRunDateTime(nextRunDateTime);
            }

            public void VerifySetAssessorSettingCalled(DateTime nextRunDateTime)
            {
                AssessorServiceApiClient.Verify(p => p.SetAssessorSetting("RefreshIlrsLastRunDate", It.Is<string>(p => p == nextRunDateTime.ToString("o"))), Times.Once);
            }
        }
    }
}
