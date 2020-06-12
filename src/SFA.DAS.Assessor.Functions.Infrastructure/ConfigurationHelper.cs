using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class ConfigurationHelper
    {
        public static string GetEnvironmentName(IConfiguration configuration)
        {
            return configuration.GetConnectionStringOrSetting("EnvironmentName");
        }

        public static string GetAppName(IConfiguration configuration)
        {
            return configuration.GetConnectionStringOrSetting("AppName");
        }

        public static List<T> ConvertCsvValueToList<T>(string csvValue)
        {
            return csvValue.Split(',').ToList().ConvertAll(p => (T)Convert.ChangeType(p, typeof(T)));
        }
    }
}
