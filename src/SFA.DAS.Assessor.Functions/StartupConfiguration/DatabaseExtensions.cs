using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options;
using System.Data;
using System.Data.SqlClient;

namespace SFA.DAS.Assessor.Functions
{
    public static class DatabaseExtensions
    {
        private const string AzureResource = "https://database.windows.net/";

        public static void AddDatabaseRegistration(this IServiceCollection services, IOptions<FunctionsOptions> options)
        {
            services.AddTransient<IDbConnection>(s =>
            {
                var sqlConnection = new SqlConnection(options.Value.SqlConnectionString);

                var useSqlConnectionMI = options?.Value.UseSqlConnectionMI ?? false;
                if (useSqlConnectionMI)
                {
                    var tokenProvider = new AzureServiceTokenProvider();
                    sqlConnection.AccessToken = tokenProvider.GetAccessTokenAsync(AzureResource).GetAwaiter().GetResult();
                }

                return sqlConnection;
            });
        }

    }
}
