using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateBatchLogPrintedRequest
    {
        public DateTime BatchDate { get; set; }
        public int PostalContactCount { get; set; }
        public int TotalCertificateCount { get; set; }
        public DateTime? PrintedDate { get; set; }
        public DateTime? DateOfResponse { get; set; }
    }
}
