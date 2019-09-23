using System;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

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
        }
    }
}