using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;
using System;
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
        Task<Dictionary<string, long>> GetLearnersWithoutEmployerInfo();
        Task<int> UpdateLearnerInfo((long uln, int standardCode, long employerAccountId, string employerName) learnerInfo);
        Task<int> InsertSearchLogsDataBase(string searchData);
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

        public async Task<Dictionary<string, long>> GetLearnersWithoutEmployerInfo()
        {
            var results = await _connection.QueryAsync<long>("SELECT DISTINCT [Uln] FROM  [dbo].[Learner] WHERE [EmployerAccountId] IS NULL");
            return results.ToDictionary(x => x.ToString(), y => y);
        }

        public async Task<int> UpdateLearnerInfo((long uln, int standardCode, long employerAccountId, string employerName) learnerInfo)
        {
            var query = new StringBuilder();
            var sqlParameters = new DynamicParameters();

            var unlParameterName = "@uln";
            var employerAccountIdParameterName = "@employerAccountId";
            var employerNameParameterName = "@employerName";
            var stdCodeParameterName = "@stdCode";

            sqlParameters.Add(unlParameterName, learnerInfo.uln, DbType.Int64);
            sqlParameters.Add(employerAccountIdParameterName, learnerInfo.employerAccountId, DbType.Int64);
            sqlParameters.Add(employerNameParameterName, learnerInfo.employerName, DbType.String);
            sqlParameters.Add(stdCodeParameterName, learnerInfo.standardCode, DbType.Int32);

            query.AppendLine($"UPDATE [Learner] SET [EmployerAccountId] = {employerAccountIdParameterName}, [EmployerName] = {employerNameParameterName} WHERE Uln = {unlParameterName} AND StdCode = {stdCodeParameterName}");
            var affectedRows = await _connection.ExecuteAsync(query.ToString(), sqlParameters);

            return affectedRows;
        }

        public async Task<int> InsertSearchLogsDataBase(string searchData)
        {
            try
            {
                var query = new StringBuilder();
                var sqlParameters = new DynamicParameters();

                var idParameterName = "@id";
                var surnameParameterName = "@surname";
                var ulnParameterName = "@uln";
                var searchTimeParameterName = "@searchTime";
                var searchDataParameterName = "@searchData";
                var numberOfResultsParameterName = "@numberOfResults";
                var userNameParameterName = "@username";

                sqlParameters.Add(idParameterName, Guid.NewGuid(), DbType.Guid);
                sqlParameters.Add(surnameParameterName, string.Empty, DbType.String);
                sqlParameters.Add(ulnParameterName, 0, DbType.Int32);
                sqlParameters.Add(searchTimeParameterName, DateTime.Now, DbType.DateTime);
                sqlParameters.Add(searchDataParameterName, searchData, DbType.String);
                sqlParameters.Add(numberOfResultsParameterName, string.Empty, DbType.String);
                sqlParameters.Add(userNameParameterName, string.Empty, DbType.String);

                query.AppendLine("INSERT INTO [dbo].[SearchLogs]([Id],[Surname],[Uln],[SearchTime],[SearchData],[NumberOfResults],[Username]) " +
                        $"VALUES({idParameterName}, {surnameParameterName}, {ulnParameterName}, {searchTimeParameterName}, {searchDataParameterName}, " +
                        $"{numberOfResultsParameterName}, {userNameParameterName})");

                var affectedRows = await _connection.ExecuteAsync(query.ToString(), sqlParameters);

                return affectedRows;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
