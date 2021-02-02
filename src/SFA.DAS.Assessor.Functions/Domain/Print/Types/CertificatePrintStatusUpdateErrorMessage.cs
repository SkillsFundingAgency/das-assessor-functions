using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class CertificatePrintStatusUpdateErrorMessage
    {
        public CertificatePrintStatusUpdate CertificatePrintStatusUpdate { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
