namespace SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance
{
    public class DatabaseMaintenanceOptions
    {
        public bool Enabled { get; set; }
        public string SqlConnectionString { get; set; }
        public bool UseSqlConnectionMI { get; set; }
    }
}
