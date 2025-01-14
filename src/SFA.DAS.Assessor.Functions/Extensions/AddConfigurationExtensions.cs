using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using System.Diagnostics;

namespace SFA.DAS.Assessor.Functions.Extensions
{
    public static class AddConfigurationsExtensions
    {
        public static void AddConfiguration(this IConfigurationBuilder builder)
        {
            var config = builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .Build();

            builder.AddAzureTableStorage(options =>
            {
                Debug.WriteLine("ConfigurationStorageConnectionString " + config["ConfigurationStorageConnectionString"]);
                Debug.WriteLine("ConfigNames " + config["ConfigNames"]);
                Debug.WriteLine("EnvironmentName " + config["EnvironmentName"]);


                options.ConfigurationKeys = config["ConfigNames"]?.Split(",") ?? [];
                options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                options.EnvironmentName = config["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }
    }
}