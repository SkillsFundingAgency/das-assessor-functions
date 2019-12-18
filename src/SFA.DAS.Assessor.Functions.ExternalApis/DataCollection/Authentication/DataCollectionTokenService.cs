using Microsoft.Extensions.Options;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication
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