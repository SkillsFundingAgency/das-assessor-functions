using SFA.DAS.Assessor.Functions.ExternalApis.Config;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorApiAuthentication : ManagedIdentityClientConfiguration
    {
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResourceId { get; set; }
        public string ApiBaseAddress { get; set; }
    }
}
