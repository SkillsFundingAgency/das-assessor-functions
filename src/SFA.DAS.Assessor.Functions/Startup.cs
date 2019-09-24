using System;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Infrastructure;

[assembly: FunctionsStartup(typeof(SFA.DAS.Assessor.Functions.WorkflowMigrator.Startup))]

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string path = fileInfo.Directory.Parent.FullName;
            LogManager.Configuration = new XmlLoggingConfiguration($@"{path}\nlog.config");
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging((loggingBuilder) => {
                loggingBuilder.AddNLog();
            });

            // var sp = builder.Services.BuildServiceProvider();

            // var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger>();
            // logger.LogInformation("WORKFLOWMIGRATE - GOT LOGGER");

            // var configuration = sp.GetService<IConfiguration>();

            // var config = new ConfigurationBuilder()
            //     .AddConfiguration(configuration)
            //     .AddAzureTableStorageConfiguration(
            //         System.Environment.GetEnvironmentVariable("ConfigurationStorageConnectionString", EnvironmentVariableTarget.Process),
            //         "SFA.DAS.Assessor.Functions",
            //         System.Environment.GetEnvironmentVariable("EnvironmentName", EnvironmentVariableTarget.Process),
            //         "1.0"
            //     ).Build();
            
            // logger.LogInformation("WORKFLOWMIGRATE - Built config");

            //builder.Services.AddOptions().Configure<SqlConnectionStrings>(config.GetSection("SqlConnectionStrings"));

            
        }
    }

    
}