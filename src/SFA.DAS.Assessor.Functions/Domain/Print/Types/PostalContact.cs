
using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class PostalContact
    {
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Department { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EmployerName { get; set; }
        
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
        public int CertificateCount { get; set; }
    }
}
