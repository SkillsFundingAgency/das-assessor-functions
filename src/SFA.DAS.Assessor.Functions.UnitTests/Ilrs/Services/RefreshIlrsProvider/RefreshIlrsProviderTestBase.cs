using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using SFA.DAS.Assessor.Functions.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ilrs.Services.RefreshIlrsProvider
{
    public class RefreshIlrsProviderTestBase
    {
        protected class TestFixture
        {
            protected RefreshIlrsProviderService Sut;

            public Mock<IOptions<RefreshIlrsOptions>> Options;
            public Mock<IDataCollectionServiceApiClient> DataCollectionServiceApiClient;
            public Mock<IRefreshIlrsAcademicYearService> MockRefreshIlrsAcademicYearsService;
            public Mock<ILogger<RefreshIlrsProviderService>> Logger;

            protected Dictionary<string, Dictionary<(DateTime, int), DataCollectionProvidersPage>> Providers =
                new Dictionary<string, Dictionary<(DateTime, int), DataCollectionProvidersPage>>();

            protected Dictionary<DateTime, List<string>> AcademicYears =
                new Dictionary<DateTime, List<string>>();

            public TestFixture WithProviders(string source, DateTime changedAt, List<int> providers, int pageSize)
            {
                var sourceDictionary = Providers.ContainsKey(source)
                    ? Providers[source]
                    : new Dictionary<(DateTime, int), DataCollectionProvidersPage>();

                var providerPages = providers.ChunkBy(pageSize);

                var dataCollectionProviderPages = providerPages.Select((p, i) => new DataCollectionProvidersPage
                {
                    Providers = p,
                    PagingInfo = new DataCollectionPagingInfo
                    {
                        PageNumber = i + 1,
                        PageSize = pageSize,
                        TotalPages = providerPages.Count,
                        TotalItems = providers.Count
                    }
                }).ToList().Append(new DataCollectionProvidersPage
                {
                    PagingInfo = new DataCollectionPagingInfo
                    {
                        PageNumber = providerPages.Count + 1,
                        PageSize = pageSize,
                        TotalPages = providerPages.Count,
                        TotalItems = providers.Count
                    }
                });

                foreach (var dataCollectionProviderPage in dataCollectionProviderPages.Select((value, index) => (value, index)))
                {
                    sourceDictionary.Add((changedAt, dataCollectionProviderPage.index + 1), dataCollectionProviderPage.value);
                }

                if (!Providers.ContainsKey(source))
                    Providers[source] = sourceDictionary;

                return this;
            }

            public TestFixture WithAcademicYear((DateTime date, List<string> academicYears) mapping)
            {
                AcademicYears
                    .Add(mapping.date, mapping.academicYears);

                return this;
            }

            public TestFixture Setup()
            {
                Options = new Mock<IOptions<RefreshIlrsOptions>>();
                Options.Setup(p => p.Value).Returns(new RefreshIlrsOptions
                {
                    ProviderPageSize = 1,
                    ProviderInitialRunDate = new DateTime(2019, 10, 10),
                    LearnerPageSize = 1,
                    LearnerFundModels = "10, 20, 30"
                });

                DataCollectionServiceApiClient = new Mock<IDataCollectionServiceApiClient>();
                DataCollectionServiceApiClient
                    .Setup(v => v.GetProviders(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync((string source, DateTime period, int? pageSize, int? pageNumber) => Providers[source][(period, pageNumber.Value)]);

                MockRefreshIlrsAcademicYearsService = new Mock<IRefreshIlrsAcademicYearService>();

                MockRefreshIlrsAcademicYearsService.Setup(v => v.ValidateAllAcademicYears(It.Is<DateTime>(p => AcademicYears.ContainsKey(p)), It.Is<DateTime>(p => AcademicYears.ContainsKey(p))))
                    .ReturnsAsync((DateTime lastRunDateTime, DateTime currentRunDateTime) => AcademicYears[lastRunDateTime].Concat(AcademicYears[currentRunDateTime]).Distinct().ToList());
                
                MockRefreshIlrsAcademicYearsService.Setup(v => v.ValidateAllAcademicYears(It.Is<DateTime>(p => AcademicYears.ContainsKey(p)), It.Is<DateTime>(p => !AcademicYears.ContainsKey(p))))
                    .ReturnsAsync((DateTime lastRunDateTime, DateTime currentRunDateTime) => AcademicYears[lastRunDateTime]);
                
                MockRefreshIlrsAcademicYearsService.Setup(v => v.ValidateAllAcademicYears(It.Is<DateTime>(p => !AcademicYears.ContainsKey(p)), It.Is<DateTime>(p => AcademicYears.ContainsKey(p))))
                    .ReturnsAsync((DateTime lastRunDateTime, DateTime currentRunDateTime) => AcademicYears[currentRunDateTime]);

                MockRefreshIlrsAcademicYearsService.Setup(v => v.ValidateAllAcademicYears(It.Is<DateTime>(p => !AcademicYears.ContainsKey(p)), It.Is<DateTime>(p => !AcademicYears.ContainsKey(p))))
                    .ThrowsAsync(new Exception());

                Logger = new Mock<ILogger<RefreshIlrsProviderService>>();

                Sut = new RefreshIlrsProviderService(
                    Options.Object,
                    DataCollectionServiceApiClient.Object,
                    MockRefreshIlrsAcademicYearsService.Object,
                    Logger.Object);

                return this;
            }

            public async Task<List<RefreshIlrsProviderMessage>> ProcessProviders(DateTime lastRunDateTime, DateTime currentRunDateTime)
            {
                return await Sut.ProcessProviders(lastRunDateTime, currentRunDateTime);
            }

            public void VerifyLogError(string message)
            {
                Logger.Verify(m => m.Log(LogLevel.Error, 0, 
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(message)), It.IsAny<Exception>(), 
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            }
        }
    }
}
