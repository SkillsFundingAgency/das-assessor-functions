using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Extensions
{
    public static class DateTimeUtils
    {
        public static DateTime UtcToTimeZoneTime(this DateTime time, string timeZoneId = "GMT Standard Time")
        {
            TimeZoneInfo tzi;
            try
            {
                tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return time;
            }
            catch (InvalidTimeZoneException)
            {
                return time;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(time, tzi);
        }
    }
}
