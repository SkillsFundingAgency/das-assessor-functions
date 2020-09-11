namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class ExternalApiDataSyncConfig
    {
        public string SqlConnectionString { get; set; }
        public string SandboxSqlConnectionString { get; set; }
        public Settings.ExternalApiDataSync ExternalApiDataSync { get; set; }
    }
}
