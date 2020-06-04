using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class CertificatePrintStatus
    {
        public string CertificateReference { get; set; }
        public int BatchNumber { get; set; }
        public string Status { get; set; }
        public DateTime StatusChangedAt { get; set; }
    }
}
