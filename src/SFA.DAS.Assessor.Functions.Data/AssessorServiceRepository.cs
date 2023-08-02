using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Infrastructure.Options;
using Z.Dapper.Plus;

namespace SFA.DAS.Assessor.Functions.Data
{
    public interface IAssessorServiceRepository
    {
        Task<Dictionary<string, long>> GetLearnersWithoutEmployerInfo();
        Task<int> UpdateLearnerInfo((long uln, int standardCode, long employerAccountId, string employerName) learnerInfo);
        int InsertIntoOfqualStagingTable(IEnumerable<IOfqualRecord> recordsToInsert);
        Task<int> ClearOfqualStagingTable(OfqualDataType ofqualDataType);
        Task<int> LoadOfqualStandards();
    }

    public class AssessorServiceRepository : IAssessorServiceRepository
    {
        private readonly IDbConnection _connection;

        public AssessorServiceRepository(IOptions<FunctionsOptions> options, IDbConnection connection)
        {
            _connection = connection;

            var useSqlConnectionMI = options?.Value.UseSqlConnectionMI ?? false;
            if (useSqlConnectionMI && _connection is SqlConnection sqlConnection)
            {
                var tokenProvider = new AzureServiceTokenProvider();
                sqlConnection.AccessToken = tokenProvider.GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();
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

        public int InsertIntoOfqualStagingTable(IEnumerable<IOfqualRecord> recordsToInsert)
        {
            DapperPlusManager.Entity<OfqualStandard>().Table("StagingOfqualStandard");
            DapperPlusManager.Entity<OfqualOrganisation>().Table("StagingOfqualOrganisation");

            var resultInfo = new Z.BulkOperations.ResultInfo();

            _connection.UseBulkOptions(o => { o.UseRowsAffected = true; o.ResultInfo = resultInfo; })
                       .BulkInsert(recordsToInsert);

            return resultInfo.RowsAffectedInserted;
        }

        public async Task<int> LoadOfqualStandards()
        {
            var parameters = new DynamicParameters();
            parameters.Add("@inserted", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync("Load_Ofqual_Standards", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@inserted");
        }

        private Task<int> ClearStagingOfqualOrganisationTable()
        {
            return _connection.ExecuteAsync("DELETE FROM [dbo].[StagingOfqualOrganisation]");
        }

        private Task<int> ClearStagingOfqualStandardTable()
        {
            return _connection.ExecuteAsync("DELETE FROM [dbo].[StagingOfqualStandard]");
        }

        public Task<int> ClearOfqualStagingTable(OfqualDataType ofqualDataType)
        {
            return ofqualDataType switch
            {
                OfqualDataType.Organisations => ClearStagingOfqualOrganisationTable(),
                OfqualDataType.Qualifications => ClearStagingOfqualStandardTable(),
                _ => throw new ArgumentException($"Could not determine which staging table to delete for Ofqual data type {ofqualDataType}")
            };
        }
    }
}