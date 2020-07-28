using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications
{
    public class DeliveryNotification
    {
        public string CertificateNumber { get; set; }
        public int BatchID { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public DateTime StatusChangeDate { get; set; }
    }
}
