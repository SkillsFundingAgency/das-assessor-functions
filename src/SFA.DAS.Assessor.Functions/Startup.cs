using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Assessor.Functions.Infrastructure;

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

            builder.Services.AddLogging((options) => {
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

            var updatedConfig = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddAzureTableStorageConfiguration(configuration["ConfigurationStorageConnectionString"], "SFA.DAS.Assessor.Functions", configuration["EnvironmentName"], "1.0")
                .Build();

            builder.Services.Configure<IConfiguration>(updatedConfig);

            builder.Services.AddOptions();
            builder.Services.Configure<AssessorApiAuthentication>(updatedConfig.GetSection("AssessorApiAuthentication"));
            builder.Services.Configure<SqlConnectionStrings>(updatedConfig.GetSection("SqlConnectionStrings"));
        }
    }
}