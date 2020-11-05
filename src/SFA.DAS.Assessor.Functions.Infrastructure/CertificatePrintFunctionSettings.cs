namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class CertificatePrintFunctionSettings
    {
        public string Schedule { get; set; }
        public string PrintRequestExternalBlobContainer { get; set; }
        public string PrintRequestInternalBlobContainer { get; set; }
        public string PrintRequestDirectory { get; set; }
        public string ArchivePrintRequestDirectory { get; set; }
        public int AddReadyToPrintChunkSize { get; set; }
        public int PrintStatusUpdateChunkSize { get; set; }
    }
}