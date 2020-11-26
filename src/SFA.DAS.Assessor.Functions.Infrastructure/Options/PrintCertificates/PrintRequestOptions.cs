namespace SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates
{
    public class PrintRequestOptions
    {
        public string Directory { get; set; }
        public string ArchiveDirectory { get; set; }
        public string ChairName { get; set; }
        public string ChairTitle { get; set; }
        public int AddReadyToPrintChunkSize { get; set; }
        public int PrintStatusUpdateChunkSize { get; set; }
    }
}