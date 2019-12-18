using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class AzureTableStorageConfigurationSource : IConfigurationSource
    {
        private readonly string _connection;
        private readonly string _environment;
        private readonly string _version;
        private readonly string _appStorageName;
        private readonly string _appName;

        public AzureTableStorageConfigurationSource(string connection, string appName, string environment, string version, string appStorageName)
        {
            _appName = appName;
            _connection = connection;
            _environment = environment;
            _version = version;
            _appStorageName = appStorageName;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AzureTableStorageConfigurationProvider(_connection,_appName, _environment, _version, _appStorageName);
        }
    }
}
