using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateScheduleRunStatusRequest
    {
        public Guid ScheduleRunId { get; set; }
        public ScheduleRunStatus ScheduleRunStatus { get; set; }
    }
    public enum ScheduleRunStatus
    {
        WaitingToStart = 0,
        Started = 1,
        Complete = 2,
        Failed = 3
    }
}
