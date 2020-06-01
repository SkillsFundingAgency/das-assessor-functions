using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public static class AzureTableStorageConfigurationExtensions
    {
        public static IConfigurationBuilder AddAzureTableStorageConfiguration(this IConfigurationBuilder builder, string connection, string appName, string environment, string version, string appStorageName)
        {
            return builder.Add(new AzureTableStorageConfigurationSource(connection,appName, environment, version, appStorageName));
        }
    }
}
