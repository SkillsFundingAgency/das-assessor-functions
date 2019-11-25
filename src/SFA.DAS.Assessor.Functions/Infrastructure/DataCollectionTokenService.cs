using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.ApiClient;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class DataCollectionTokenService : IDataCollectionTokenService
    {
        private readonly IDataCollectionServiceAnonymousApiClient _dataCollectionServiceAnonymousApiClient;
        private readonly IOptions<DataCollectionApiAuthentication> _dataCollectionApiAuthenticationOptions;
        

        public DataCollectionTokenService(IDataCollectionServiceAnonymousApiClient dataCollectionServiceAnonymousApiClient, IOptions<DataCollectionApiAuthentication> dataCollectionApiAuthenticationOptions)
        {
            _dataCollectionServiceAnonymousApiClient = dataCollectionServiceAnonymousApiClient;
            _dataCollectionApiAuthenticationOptions = dataCollectionApiAuthenticationOptions;
        }

        public string GetToken()
        {
            var tokenRequest = new DataCollectionTokenRequest
            {
                Username = _dataCollectionApiAuthenticationOptions?.Value.Username,
                Password = _dataCollectionApiAuthenticationOptions?.Value.Password
            };

            var t = _dataCollectionServiceAnonymousApiClient.GetToken(tokenRequest);

            return t.Result;
        }
    }
}