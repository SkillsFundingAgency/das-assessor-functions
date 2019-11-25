using SFA.DAS.Assessor.Functions.Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ApiClient
{
    public interface IDataCollectionServiceApiClient
    {
        HttpClient Client { get; }

        Task<List<int>> GetProviders(DateTime startDateTime);
        Task<List<DataCollectionLearner>> GetLearners(int ukprn);
    }
}