using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Authentication
{
    public class SecureMessageTokenService : ISecureMessageTokenService
    {
        private readonly SecureMessageApiAuthentication _secureMessageApiAuthentication;

        private string _accessToken = null;

        public SecureMessageTokenService(IOptions<SecureMessageApiAuthentication> options)
        {
            _secureMessageApiAuthentication = options?.Value;
        }

        public async Task<string> GetToken()
        {
            if (_accessToken != null)
                return _accessToken;

            if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
            {
                // When running locally cannot use Managed Identity (MI) as it does not support user assigned permissions
                var tenantId = _secureMessageApiAuthentication.OAuth.TenantId;
                var clientId = _secureMessageApiAuthentication.OAuth.ClientId;
                var clientSecret = _secureMessageApiAuthentication.OAuth.ClientSecret;
                var authority = $"{_secureMessageApiAuthentication.OAuth.Instance}{tenantId}";
                var resourceId = _secureMessageApiAuthentication.ResourceId;
                
                var clientCredential = new ClientCredential(clientId, clientSecret);
                var context = new AuthenticationContext(authority, true);
                var result = await context.AcquireTokenAsync(resourceId, clientCredential);

                _accessToken = result.AccessToken;
            }
            else
            {
                // When running in Azure use Managed Identity (MI) which is recommended AS Technical Guidance
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(_secureMessageApiAuthentication.ResourceId);

                return accessToken;
            }

            return _accessToken;
        }

        public async Task<string> RefreshToken()
        {
            _accessToken = null;
            return await GetToken();
        }
    }
}
