using MediatR;
using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class CreateBatchLogRequest : IRequest<BatchLogResponse>
    {
        public DateTime BatchCreated { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Period { get; set; }
        public int BatchNumber { get; set; }
        public int NumberOfCertificates { get; set; }
        public int NumberOfCoverLetters { get; set; }
        public string CertificatesFileName { get; set; }
        public DateTime FileUploadStartTime { get; set; }
        public DateTime FileUploadEndTime { get; set; }
    }
}
