using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.IntegrationTests.DataCollection
{
    public class When_academic_years_are_requested_sources_are_returned 
    {
        protected Mock<IOptions<DataCollectionApiAuthentication>> DataCollectionOptions;
        protected Mock<ILogger<DataCollectionServiceApiClient>> DataCollectionServiceApiClientLogger;

        protected DataCollectionServiceApiClient Sut;

        [SetUp]
        public void Arrange()
        {
            // read configuration from azure storage
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureTableStorageConfiguration(
                    "UseDevelopmentStorage=true",
                    string.Empty,
                    "LOCAL",
                    "1.0", "SFA.DAS.AssessorFunctions")
                .Build();

            var dataCollectionApiAuthentication = config.GetSection("DataCollectionApiAuthentication");
            
            // create a mock options with azure configuration as a workaround for internal options constructor
            DataCollectionOptions = new Mock<IOptions<DataCollectionApiAuthentication>>();
            DataCollectionOptions.Setup(p => p.Value).Returns(new DataCollectionApiAuthentication
            {
                TenantId = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.TenantId)],
                ClientId = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.ClientId)],
                ClientSecret = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.ClientSecret)],
                Scope = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.Scope)],
                Version = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.Version)],
                ApiBaseAddress = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.ApiBaseAddress)],
            });

            DataCollectionServiceApiClientLogger = new Mock<ILogger<DataCollectionServiceApiClient>>();

            // remove ssl client certfication checks
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };

            Sut = new DataCollectionServiceApiClient(
                new HttpClient(handler), 
                new DataCollectionTokenService(DataCollectionOptions.Object), 
                DataCollectionOptions.Object, 
                DataCollectionServiceApiClientLogger.Object);
        }
        
        [TestCase("01/02/2019", "1819")]
        [TestCase("22/08/2019", "1819")]
        [TestCase("23/08/2019", "1819,1920")] // These rollover dates are known by experimentation but do not align with published period dates
        [TestCase("17/10/2019", "1819,1920")]
        [TestCase("18/10/2019", "1920")]
        [TestCase("22/08/2020", "1920")]
        [TestCase("23/08/2020", "1920,2021")] // These rollover dates are guessed based on previous values, currently they fail
        [TestCase("17/10/2020", "1920,2021")]
        [TestCase("18/10/2020", "2021")]
        [TestCase("01/02/2021", "2021")]
        public async Task Then_correct_sources_are_retrieved(string dateTimeString, string expectedSources)
        {
            // NOTE: These tests were originally written under the assumption that the Academic Years were calculated
            // based on examples of currently known period dates; but the DC API actually calculates them based on data 
            // which may or may not be present; so it is expected that ones which currently pass may fail in the future
            // at which point they could be removed or replaced with other examples of known period dates.

            // Arrange
            var dateTime = DateTime.Parse(dateTimeString);

            // Act
            var sources = await Sut.GetAcademicYears(dateTime);

            // Assert
            sources.Should().Equal(expectedSources.Split(',').ToList());
        }
    }
}
