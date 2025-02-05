using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;
using System.Diagnostics;

namespace SFA.DAS.Assessor.Functions.Extensions
{
    public static class AddConfigurationsExtensions
    {
        public static void AddConfiguration(this IConfigurationBuilder builder)
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile("local.settings.json", optional: true)
#endif
                .AddEnvironmentVariables();
            
            var config = builder.Build();

            builder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = config["ConfigNames"]?.Split(",") ?? [];
                options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                options.EnvironmentName = config["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }
    }
}