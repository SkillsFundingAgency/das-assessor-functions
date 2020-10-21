namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class CertificatePrintNotificationFunctionSettings
    {
        public string Schedule { get; set; }
        public string PrintResponseExternalBlobContainer { get; set; }
        public string PrintResponseInternalBlobContainer { get; set; }
        public string PrintResponseDirectory { get; set; }
        public string ArchivePrintResponseDirectory { get; set; }
        public string ErrorArchivePrintResponseDirectory { get; set; }
    }
}
