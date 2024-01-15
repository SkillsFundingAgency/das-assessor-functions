using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Http.TokenGenerators;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorServiceTokenService : IAssessorServiceTokenService
    {
        private readonly IManagedIdentityTokenGenerator _managedIdentityTokenGenerator;

        private string _accessToken = null;

        public AssessorServiceTokenService(IManagedIdentityTokenGenerator managedIdentityTokenGenerator)
        {
            _managedIdentityTokenGenerator = managedIdentityTokenGenerator;
        }

        public async Task<string> GetToken()
        {
            if (_accessToken != null)
                return _accessToken;
            var isLocal = false;
            if (isLocal && string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
            {
                _accessToken = string.Empty;
            }
            else
            {
                //var defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
                //var result = await defaultAzureCredential.GetTokenAsync(
                //    new TokenRequestContext(scopes: new string[] { _assessorApiAuthenticationOptions.IdentifierUri + "/.default" }) { });

                _accessToken = await _managedIdentityTokenGenerator.Generate();
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
