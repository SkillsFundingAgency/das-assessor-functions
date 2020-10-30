using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class CertificatePrintStatusUpdateMessage
    {
        public List<CertificatePrintStatusUpdate> CertificatePrintStatusUpdates { get; set; }
    }
}
