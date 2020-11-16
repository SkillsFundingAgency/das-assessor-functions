namespace SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates
{
    public class BlobSasTokenGeneratorOptions
    {
        public int SasExpiryDays { get; set; }
        public string SasIPAddress { get; set; }
		public string SecureMessageTtl { get; set; }
    }
}