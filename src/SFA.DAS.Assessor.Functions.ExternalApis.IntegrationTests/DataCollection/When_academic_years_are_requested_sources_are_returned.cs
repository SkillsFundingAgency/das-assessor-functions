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
        protected Mock<ILogger<DataCollectionServiceAnonymousApiClient>> DataCollectionServiceAnonymousApiClientLogger;
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
                Username = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.Username)],
                Password = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.Password)],
                Version = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.Version)],
                ApiBaseAddress = dataCollectionApiAuthentication[nameof(DataCollectionApiAuthentication.ApiBaseAddress)],
            });

            DataCollectionServiceApiClientLogger = new Mock<ILogger<DataCollectionServiceApiClient>>();
            DataCollectionServiceAnonymousApiClientLogger = new Mock<ILogger<DataCollectionServiceAnonymousApiClient>>();

            // remove ssl client certfication checks
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };

            var dataCollectionServiceAnonymousApiClient = new DataCollectionServiceAnonymousApiClient(
                new HttpClient(handler), 
                DataCollectionOptions.Object, 
                DataCollectionServiceAnonymousApiClientLogger.Object);

            Sut = new DataCollectionServiceApiClient(
                new HttpClient(handler), 
                new DataCollectionTokenService(dataCollectionServiceAnonymousApiClient, DataCollectionOptions.Object), 
                DataCollectionOptions.Object, 
                DataCollectionServiceApiClientLogger.Object);
        }

        [TestCase("2019-10-10", "1920")]
        [TestCase("01/02/2020", "1920")]
        [TestCase("06/08/2020", "1920,2021")]
        [TestCase("06/10/2010", "2021")]
        [TestCase("01/03/2021", "2021")]
        public async Task Then_learner_details_are_retrieved(string dateTimeString, string expectedSources)
        {
            // Arrange
            var dateTime = DateTime.Parse(dateTimeString);

            // Act
            var sources = await Sut.GetAcademicYears(dateTime);

            // Assert
            sources.Should().Equal(expectedSources.Split(',').ToList());
        }
    }
}
