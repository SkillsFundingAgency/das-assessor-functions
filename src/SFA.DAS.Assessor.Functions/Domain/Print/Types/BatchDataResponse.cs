using Newtonsoft.Json;
using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class BatchDataResponse
    {
        public string BatchNumber { get; set; }
        public DateTime BatchDate { get; set; }
        public int PostalContactCount { get; set; }
        public int TotalCertificateCount { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PrintedDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PostedDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateOfResponse { get; set; }
    }
}
