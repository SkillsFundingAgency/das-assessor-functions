using MediatR;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateCertificatesBatchToIndicatePrintedRequest : IRequest
    {
        public int BatchNumber { get; set; }
        public List<CertificateStatus> CertificateStatuses { get; set; }
    }
}
