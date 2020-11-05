namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class CertificateDeliveryNotificationFunctionSettings
    {
        public string Schedule { get; set; }
        public string DeliveryNotificationExternalBlobContainer { get; set; }
        public string DeliveryNotificationInternalBlobContainer { get; set; }
        public string DeliveryNotificationDirectory { get; set; }
        public string ArchiveDeliveryNotificationDirectory { get; set; }
        public string ErrorDeliveryNotificationDirectory { get; set; }
		public int PrintStatusUpdateChunkSize { get; set; }
    }
}
