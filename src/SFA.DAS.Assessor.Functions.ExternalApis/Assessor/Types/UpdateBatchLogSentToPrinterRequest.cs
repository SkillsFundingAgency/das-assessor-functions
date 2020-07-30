using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateBatchLogSentToPrinterRequest
    {
        public int BatchNumber { get; set; }
        public List<string> CertificateReferences { get; set; }
    }
}
