using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection
{
    public interface IDataCollectionServiceAnonymousApiClient
    {
        Task<string> GetToken(DataCollectionTokenRequest request);
    }
}