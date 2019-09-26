using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Newtonsoft.Json;

[assembly: FunctionsStartup(typeof(SFA.DAS.Assessor.Functions.WorkflowMigrator.Startup))]

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var sp = builder.Services.BuildServiceProvider();

            var configuration = sp.GetService<IConfiguration>();

            var nLogConfiguration = new NLogConfiguration();

            builder.Services.AddLogging((options) =>
            {
                options.SetMinimumLevel(LogLevel.Trace);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog(configuration);
            });

            // Get config json

            var storageAccount = CloudStorageAccount.Parse(configuration["ConfigurationStorageConnectionString"]);
            var tableClient = storageAccount.CreateCloudTableClient().GetTableReference("Configuration");
            var operation = TableOperation.Retrieve<ConfigurationItem>(configuration["EnvironmentName"], $"SFA.DAS.Assessor.Functions_1.0");

            var result = tableClient.Execute(operation).Result;

            var configItem = (ConfigurationItem)result;

            var functionsConfig = JsonConvert.DeserializeObject<FunctionsConfiguration>(configItem.Data);
        }
    }
}