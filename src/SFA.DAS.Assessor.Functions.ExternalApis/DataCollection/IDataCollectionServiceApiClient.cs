using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection
{
    public interface IDataCollectionServiceApiClient : IApiClientBase
    {
        Task<List<string>> GetAcademicYears(DateTime dateTimeUtc);
        Task<DataCollectionProvidersPage> GetProviders(string source, DateTime startDateTime, int? pageSize = null, int? pageNumber = null);
        Task<DataCollectionLearnersPage> GetLearners(string source, DateTime startDateTime, int? aimType = null, int? standardCode = null, List<int> fundModels = null, int? pageSize = null, int? pageNumber = null);
        Task<DataCollectionLearnersPage> GetLearners(string source, int ukprn, int? aimType = null, int? standardCode = null, List<int> fundModels = null, int? pageSize = null, int? pageNumber = null);
    }
}