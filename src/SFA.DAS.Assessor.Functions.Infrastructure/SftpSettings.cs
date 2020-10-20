namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class SftpSettings
    {
        public string RemoteHost { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string UploadDirectory { get; set; }
        public string ProofDirectory { get; set; }
        public string PrintRequestDirectory { get; set; }
        public string PrintResponseDirectory { get; set; }
        public string DeliveryNotificationDirectory { get; set; }
        public string ArchivePrintResponseDirectory { get; set; }
        public string ArchiveDeliveryNotificationDirectory { get; set; }
        public string ErrorPrintResponseDirectory { get; set; }
        public string ErrorDeliveryNotificationDirectory { get; set; }
        public bool UseJson { get; set; }
    }
}