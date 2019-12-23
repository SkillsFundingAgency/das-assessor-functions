using System;

namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public class MigrationError
    {
        public Guid? OriginalApplicationId { get; set; }
        public string Reason { get; set; }
    }
}