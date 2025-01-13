using Microsoft.Azure.Functions.Worker;
using System;

namespace SFA.DAS.Assessor.Functions.UnitTests.Helpers;

public static class TimerInfoFactory
{
    public static TimerInfo Create(
        DateTime? last = null,
        DateTime? next = null,
        bool isPastDue = true)
    {
        var scheduleStatus = new ScheduleStatus
        {
            Last = last ?? DateTime.UtcNow.AddMinutes(-5),
            Next = next ?? DateTime.UtcNow.AddMinutes(5),
            LastUpdated = DateTime.UtcNow
        };
        return new TimerInfo
        {
            ScheduleStatus = scheduleStatus,
            IsPastDue = true
        };
    }
}
