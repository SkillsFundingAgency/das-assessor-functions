using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications
{
    public class BatchData
    {
        public string BatchNumber { get; set; }
        public DateTime BatchDate { get; set; }
        public int PostalContactCount { get; set; }
        public int TotalCertificateCount { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
}
