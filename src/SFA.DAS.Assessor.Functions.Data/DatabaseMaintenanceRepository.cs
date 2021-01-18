using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Data
{
    public class DatabaseMaintenanceRepository : IDatabaseMaintenanceRepository
    {
        private readonly IDbConnection _connection;

        public DatabaseMaintenanceRepository(IOptions<DatabaseMaintenanceOptions> options, IDbConnection connection)
        {
            _connection = connection;

            var useSqlConnectionMI = options?.Value.UseSqlConnectionMI ?? false;
            if (useSqlConnectionMI && _connection is SqlConnection sqlConnection)
            {
                var tokenProvider = new AzureServiceTokenProvider();
                sqlConnection.AccessToken = tokenProvider.GetAccessTokenAsync("https://database.windows.net/").Result;
            }
        }

        public async Task<List<string>> DatabaseMaintenance()
        {
            var results = await _connection.QueryAsync<string>("DatabaseMaintenace",
                commandType: CommandType.StoredProcedure);

            return results
                .ToList();
        }
    }
}
