using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorServiceTokenService : IAssessorServiceTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AssessorApiAuthentication _assessorApiAuthenticationOptions;

        private string _accessToken = null;

        public AssessorServiceTokenService(IOptions<AssessorApiAuthentication> options, IConfiguration configuration)
        {
            _assessorApiAuthenticationOptions = options?.Value;
            _configuration = configuration;
        }

        public async Task<string> GetToken()
        {
            if (_accessToken != null)
                return _accessToken;

            if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
            {
                _accessToken = string.Empty;
            }
            else
            {
                var defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
                var result = await defaultAzureCredential.GetTokenAsync(
                    new TokenRequestContext(scopes: new string[] { _assessorApiAuthenticationOptions.IdentifierUri + "/.default" }) { });

                _accessToken = result.Token;
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
