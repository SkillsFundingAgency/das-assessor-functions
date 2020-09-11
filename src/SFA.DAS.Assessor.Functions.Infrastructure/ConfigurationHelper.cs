using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public static class ConfigurationHelper
    {
        public static string GetEnvironmentName(IConfiguration configuration)
        {
            return configuration.GetConnectionStringOrSetting("EnvironmentName");
        }

        public static string GetAppName(IConfiguration configuration)
        {
            return configuration.GetConnectionStringOrSetting("AppName");
        }
    }
}
