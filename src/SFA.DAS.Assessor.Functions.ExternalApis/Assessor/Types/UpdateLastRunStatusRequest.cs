using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateLastRunStatusRequest
    {
        public Guid ScheduleRunId { get; set; }
        public LastRunStatus LastRunStatus { get; set; }
    }

    public enum LastRunStatus
    {
        Restarting = 0,
        Started = 1,
        Completed = 2,
        Failed = 3
    }
}
