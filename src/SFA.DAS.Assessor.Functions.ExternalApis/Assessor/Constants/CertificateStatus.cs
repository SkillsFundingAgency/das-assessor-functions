using System.Linq;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants
{
    public class CertificateStatus
    {
        public const string SentToPrinter = "SentToPrinter";
        public const string Printed = "Printed";
        public const string Delivered = "Delivered";
        public const string NotDelivered = "NotDelivered";

        public static string[] DeliveryNotificationStatus = new[] { Delivered, NotDelivered };

        public static bool HasDeliveryNotificationStatus(string status)
        {
            return DeliveryNotificationStatus.Contains(status);
        }
    }
}
