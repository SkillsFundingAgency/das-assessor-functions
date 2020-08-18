using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Extensions
{
    public static class ListExtensions
    {
        public static List<string> SortByDateTimePattern(this List<string> items, string dateTimePattern, string dateTimeFormat)
        {
            var sortedItems = items;
            sortedItems.Sort((item1, item2) =>
            {
                if (ParseDateTime(item1, dateTimePattern, dateTimeFormat, out DateTime item1DateTime) &&
                    ParseDateTime(item2, dateTimePattern, dateTimeFormat, out DateTime item2DateTime))
                {
                    return item1DateTime.CompareTo(item2DateTime);
                }

                return 0;
            });
            
            return sortedItems;
        }

        private static bool ParseDateTime(string item, string dateTimePattern, string dateTimeFormat, out DateTime datetime)
        {
            return DateTime.TryParseExact(
                Regex.Match(item, dateTimePattern)?.Groups[0].Value, new string[] { dateTimeFormat }, 
                null, 
                System.Globalization.DateTimeStyles.None, 
                out datetime);
        }
    }
}
