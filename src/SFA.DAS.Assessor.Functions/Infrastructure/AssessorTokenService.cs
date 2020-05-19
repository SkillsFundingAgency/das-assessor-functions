//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
//using SFA.DAS.Assessor.Functions.Interfaces;

//namespace SFA.DAS.Assessor.Functions.Infrastructure
//{
//    public class AssessorTokenService : ITokenService
//    {
//        private readonly AssessorApiAuthentication _assessorApiAuthenticationOptions;
//        private readonly IConfiguration _configuration;

//        public AssessorTokenService(AssessorApiAuthentication assessorApiAuthenticationOptions, IConfiguration configuaration)
//        {
//            _assessorApiAuthenticationOptions = assessorApiAuthenticationOptions;
//            _configuration = configuaration;
//        }

//        public string GetToken()
//        {
//            if (string.Equals("LOCAL", ConfigurationHelper.GetEnvironmentName(_configuration)))
//                return string.Empty;

//            var tenantId = _assessorApiAuthenticationOptions.TenantId;
//            var clientId = _assessorApiAuthenticationOptions.ClientId;
//            var appKey = _assessorApiAuthenticationOptions.ClientSecret;
//            var resourceId = _assessorApiAuthenticationOptions.ResourceId;

//            var authority = $"https://login.microsoftonline.com/{tenantId}";
//            var clientCredential = new ClientCredential(clientId, appKey);
//            var context = new AuthenticationContext(authority, true);
//            var result = context.AcquireTokenAsync(resourceId, clientCredential).Result;

//            return result.AccessToken;
//        }
//    }
//}