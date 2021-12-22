using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Data
{
    public interface IAssessorServiceRepository
    {
        Task<List<long>> GetLearnersWithoutEmployerInfo();

        Task UpdateLeanerInfo(List<(long uln, int standardCode, long employerAccountId, string employerName)> learnersInfos);
    }

    public class AssessorServiceRepository : IAssessorServiceRepository
    {
        private readonly IDbConnection _connection;

        public AssessorServiceRepository(IOptions<DatabaseMaintenanceOptions> options, IDbConnection connection)
        {
            _connection = connection;

            var useSqlConnectionMI = options?.Value.UseSqlConnectionMI ?? false;
            if (useSqlConnectionMI && _connection is SqlConnection sqlConnection)
            {
                var tokenProvider = new AzureServiceTokenProvider();
                sqlConnection.AccessToken = tokenProvider.GetAccessTokenAsync("https://database.windows.net/").Result;
            }
        }

        public async Task<List<long>> GetLearnersWithoutEmployerInfo()
        {
            var results = await _connection.QueryAsync<long>("SELECT DISTINCT [Uln] FROM  [dbo].[Learner] WHERE [EmployerAccountId] IS NULL");
            return results.ToList();
        }

        public async Task UpdateLeanerInfo(List<(long uln, int standardCode, long employerAccountId, string employerName)> learnersInfos)
        {
            var query = new StringBuilder();
            var sqlParameters = new DynamicParameters();

            int i = 1;
            int parameterCount = 0;
            foreach (var learnerInfo in learnersInfos)
            {

                var unlParameterName = "@uln" + i;
                var employerAccountIdParameterName = "@employerAccountId" + i;
                var employerNameParameterName = "@employerName" + i;
                var stdCodeParameterName = "@stdCode" + i;

                AddSqlParameters(unlParameterName, learnerInfo.uln, DbType.Int64, ref parameterCount, ref sqlParameters);
                AddSqlParameters(employerAccountIdParameterName, learnerInfo.employerAccountId, DbType.Int64, ref parameterCount, ref sqlParameters);
                AddSqlParameters(employerNameParameterName, learnerInfo.employerName, DbType.String, ref parameterCount, ref sqlParameters);
                AddSqlParameters(stdCodeParameterName, learnerInfo.standardCode, DbType.Int32, ref parameterCount, ref sqlParameters);

                query.AppendLine($"UPDATE [Learner] SET [EmployerAccountId] = {employerAccountIdParameterName}, [EmployerName] = {employerNameParameterName} WHERE Uln = {unlParameterName} AND StdCode = {stdCodeParameterName}");

                if (parameterCount == 2000) // max allowed sql parameters
                {
                    await _connection.ExecuteAsync(query.ToString(), sqlParameters);

                    sqlParameters = new DynamicParameters();
                    query.Length = 0;
                }

                i++;
            }

            if (query.Length > 0)
            {
                await _connection.ExecuteAsync(query.ToString(), sqlParameters);
            }

        }

        private void AddSqlParameters( string name, object value , DbType dbType, ref int count, ref DynamicParameters sqlParameters)
        {
            sqlParameters.Add(name, value, dbType);
            count++;
        }

    }
}
