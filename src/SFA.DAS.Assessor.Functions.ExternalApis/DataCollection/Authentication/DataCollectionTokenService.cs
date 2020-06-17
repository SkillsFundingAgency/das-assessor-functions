using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication
{
    public class DataCollectionTokenService : IDataCollectionTokenService
    {
        private readonly DataCollectionApiAuthentication _dataCollectionApiAuthenticationOptions;
        private string _accessToken = null;

        public DataCollectionTokenService(IOptions<DataCollectionApiAuthentication> dataCollectionApiAuthenticationOptions)
        {
            _dataCollectionApiAuthenticationOptions = dataCollectionApiAuthenticationOptions?.Value;    
        }

        public async Task<string> GetToken()
        {
            if (_accessToken != null)
                return _accessToken;

            var clientId = _dataCollectionApiAuthenticationOptions.ClientId;
            var clientSecret = _dataCollectionApiAuthenticationOptions.ClientSecret;
            var scope = _dataCollectionApiAuthenticationOptions.Scope;

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(_dataCollectionApiAuthenticationOptions.Authority))
                    .Build();

            string[] scopes = new string[] { scope };

            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new Exception("Invalid scope specified", ex);
            }

            _accessToken = result?.AccessToken;
            return _accessToken;
        }

        public async Task<string> RefreshToken()
        {
            _accessToken = null;
            return await GetToken();
        }
    }
}