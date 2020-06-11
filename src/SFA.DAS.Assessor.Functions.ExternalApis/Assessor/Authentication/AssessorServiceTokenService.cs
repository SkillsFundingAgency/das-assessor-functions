using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorServiceTokenService : IAssessorServiceTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AssessorApiAuthentication _assessorApiAuthenticationOptions;

        public AssessorServiceTokenService(IOptions<AssessorApiAuthentication> options, IConfiguration configuaration)
        {
            _assessorApiAuthenticationOptions = options?.Value;
            _configuration = configuaration;
        }

        public string GetToken()
        {
            if (string.Equals("LOCAL", ConfigurationHelper.GetEnvironmentName(_configuration)))
                return string.Empty;

            var tenantId = _assessorApiAuthenticationOptions.TenantId;
            var clientId = _assessorApiAuthenticationOptions.ClientId;
            var appKey = _assessorApiAuthenticationOptions.ClientSecret;
            var resourceId = _assessorApiAuthenticationOptions.ResourceId;

            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var clientCredential = new ClientCredential(clientId, appKey);
            var context = new AuthenticationContext(authority, true);
            var result = context.AcquireTokenAsync(resourceId, clientCredential).Result;

            return result.AccessToken;
        }
    }
}
