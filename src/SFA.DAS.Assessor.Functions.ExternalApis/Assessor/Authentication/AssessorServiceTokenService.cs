﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorServiceTokenService : IAssessorServiceTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AssessorApiAuthentication _assessorApiAuthenticationOptions;

        private string _accessToken = null;

        public AssessorServiceTokenService(IOptions<AssessorApiAuthentication> options, IConfiguration configuaration)
        {
            _assessorApiAuthenticationOptions = options?.Value;
            _configuration = configuaration;
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
                var tenantId = _assessorApiAuthenticationOptions.TenantId;
                var clientId = _assessorApiAuthenticationOptions.ClientId;
                var appKey = _assessorApiAuthenticationOptions.ClientSecret;
                var resourceId = _assessorApiAuthenticationOptions.ResourceId;

                var authority = $"https://login.microsoftonline.com/{tenantId}";
                var clientCredential = new ClientCredential(clientId, appKey);
                var context = new AuthenticationContext(authority, true);
                var result = await context.AcquireTokenAsync(resourceId, clientCredential);

                _accessToken = result.AccessToken;
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
