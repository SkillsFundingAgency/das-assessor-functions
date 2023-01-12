using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
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

            // using MI (Managed Identity) configured for the Azure service to service authentication
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            _accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(_secureMessageApiAuthentication.ResourceId);

            return _accessToken;
        }

        public async Task<string> RefreshToken()
        {
            _accessToken = null;
            return await GetToken();
        }
    }
}
