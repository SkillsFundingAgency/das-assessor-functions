using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Config
{
    public class ManagedIdentityClientConfiguration : IManagedIdentityClientConfiguration
    {
        public string IdentifierUri { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}
