namespace SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates
{
    public class PrintResponseOptions
    {
        public string Directory { get; set; }
        public string ArchiveDirectory { get; set; }
        public string ErrorDirectory { get; set; }
        public int PrintStatusUpdateChunkSize { get; set; }
    }
}
