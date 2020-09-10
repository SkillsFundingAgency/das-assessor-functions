using StructureMap;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Logger;

namespace SFA.DAS.Assessor.Functions
{
    public static class Bootstrapper
    {
        private static IAggregateLogger _logger;

        public static void StartUp(ILogger functionLogger, ExecutionContext context)
        {
            _logger = new AggregateLogger("das-assessor-service-func-externalapidatasync", functionLogger, context);

            var configuration = ConfigurationHelper.GetConfiguration();

            _logger.LogInformation("Initialising bootstrapper ....");
            _logger.LogInformation("Config Received");

            Container = new Container(configure =>
            {
                configure.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });

                configure.For<IAggregateLogger>().Use(_logger).Singleton();
                configure.For<IWebConfiguration>().Use(configuration).Singleton();
                configure.For<IDbConnection>().Use(c => new SqlConnection(configuration.SqlConnectionString));
            });

            var language = "en-GB";
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
        }

        public static Container Container { get; private set; }
    }
}
