using System.Collections.Generic;


namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public class MigrationResult
    {
        public int NumberOfApplicationsToMigrate {get;set;}
        public int NumberOfApplicationsMigrated {get;set;}

        public List<MigrationError> MigrationErrors {get;set;}
    }
}