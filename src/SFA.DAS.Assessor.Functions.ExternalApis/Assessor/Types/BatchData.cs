using Newtonsoft.Json;
using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class BatchData
    {
        public int BatchNumber { get; set; }
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
