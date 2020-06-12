using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication
{
    public class DataCollectionTokenService : IDataCollectionTokenService
    {
        private readonly DataCollectionApiAuthentication _dataCollectionApiAuthenticationOptions;
                
        public DataCollectionTokenService(IOptions<DataCollectionApiAuthentication> dataCollectionApiAuthenticationOptions)
        {
            _dataCollectionApiAuthenticationOptions = dataCollectionApiAuthenticationOptions?.Value;    
        }

        public string GetToken()
        {
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
                result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result;
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new Exception("Invalid scope specified", ex);
            }

            return result?.AccessToken;
        }
    }
}