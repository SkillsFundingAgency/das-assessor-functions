using SFA.DAS.Assessor.Functions.Domain;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ApiClient
{
    public interface IDataCollectionServiceAnonymousApiClient
    {
        HttpClient Client { get; }

        Task<string> GetToken(DataCollectionTokenRequest request);
    }
}