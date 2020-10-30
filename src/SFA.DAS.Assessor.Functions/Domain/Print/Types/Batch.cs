using System;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class Batch
    {
        public Guid? Id { get; set; }
        public string Status { get; set; }
        public int BatchNumber { get; set; }
        public DateTime FileUploadStartTime { get; set; }
        public string Period { get; set; }
        public DateTime BatchCreated { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string CertificatesFileName { get; set; }
        public DateTime FileUploadEndTime { get; set; }
        public int NumberOfCertificates { get; set; }
        public int NumberOfCoverLetters { get; set; }
        public DateTime? PrintedDate { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? DateOfResponse { get; set; }
        public List<Certificate> Certificates { get; set; }
    }
}
