using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Http.TokenGenerators;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorServiceTokenService : IAssessorServiceTokenService
    {
        private readonly IManagedIdentityTokenGenerator _managedIdentityTokenGenerator;
        private readonly ILogger<AssessorServiceTokenService> _logger;
        private string _accessToken = null;

        public AssessorServiceTokenService(IManagedIdentityTokenGenerator managedIdentityTokenGenerator, ILogger<AssessorServiceTokenService> logger)
        {
            _managedIdentityTokenGenerator = managedIdentityTokenGenerator;
            _logger = logger;
        }

        public async Task<string> GetToken()
        {
            try
            {
                if (_accessToken != null)
                    return _accessToken;

                if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
                {
                    _accessToken = string.Empty;
                }
                else
                {
                    _accessToken = await _managedIdentityTokenGenerator.Generate();
                    _logger.LogInformation($"Successfully generated token in AssessorServiceTokenService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating token in AssessorServiceTokenService {ex.Message}");
                throw;
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