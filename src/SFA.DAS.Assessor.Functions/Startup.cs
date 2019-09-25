using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;

[assembly: FunctionsStartup(typeof(SFA.DAS.Assessor.Functions.WorkflowMigrator.Startup))]

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging((options) => {
                options.SetMinimumLevel(LogLevel.Trace);
                //options.AddNLog(new NLogProviderOptions{});
                options.AddConsole();
                options.AddDebug();
            });
        }
    }
}