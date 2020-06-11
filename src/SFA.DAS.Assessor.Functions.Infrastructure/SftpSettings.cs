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
        public bool UseJson { get; set; }
    }
}