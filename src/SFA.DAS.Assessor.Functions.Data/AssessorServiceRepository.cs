﻿using Dapper;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Data
{
    /// <summary>
    /// This class contains methods which directly access the Assessor database.
    /// 
    /// This is an exception to the general rule of allowing only the API for each service to connect 
    /// with the database for a given service. The justification being that writing large amounts of data 
    /// into a database is overly complicated when using an API endpoint in conjunction with message queuing
    /// which is the usual pattern commonly followed to distribute load over a period and thus avoid function timeouts.
    /// 
    /// This approach was implemented at the behest of the Technical Authority who has given an architectural waiver 
    /// for this deviation from the usual pattern.
    ///
    /// There should be no logic contained in any SQL statements included in this class e.g. not even a
    /// predicate should be allowed. If logic is required to be triggered after data loading e.g. to 
    /// transform staging data then a Stored Procedure should be called to perform that logic. This strikes
    /// a balance between reducing the complexity of the service and degrading the maintainability.
    /// </summary>
    public class AssessorServiceRepository : IAssessorServiceRepository
    {
        private readonly IDbConnection _connection;

        public AssessorServiceRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<Dictionary<string, long>> GetLearnersWithoutEmployerInfo()
        {
            // this is breaking the rule about not including logic in this class by having a predicate in the SQL statement. It MUST not be used to jusify adding other logic to this class
            var results = await _connection.QueryAsync<long>("SELECT DISTINCT [Uln] FROM  [dbo].[Learner] WHERE [EmployerAccountId] IS NULL");
            return results.ToDictionary(x => x.ToString(), y => y);
        }

        public async Task<int> UpdateLearnerInfo((long uln, int standardCode, long employerAccountId, string employerName) learnerInfo)
        {
            // this is breaking the rule about not including logic in this class by having a predicate in the SQL statement. It MUST not be used to jusify adding other logic to this class
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

        public int InsertIntoOfqualStagingTable(IEnumerable<IOfqualRecord> recordsToInsert, OfqualDataType ofqualDataType)
        {
            switch(ofqualDataType)
            {
                case OfqualDataType.Organisations:
                    return InsertIntoOfqualOrganisationStagingTable(recordsToInsert.Select(r => r as OfqualOrganisation));
                case OfqualDataType.Qualifications:
                    return InsertIntoOfqualQualificationStagingTable(recordsToInsert.Select(r => r as OfqualStandard));
                default:
                    throw new ArgumentException($"{ofqualDataType} is an unknown Ofqual data type.");
            }
        }

        private int InsertIntoOfqualOrganisationStagingTable(IEnumerable<OfqualOrganisation> recordsToInsert) 
        {
            int recordsInserted = 0;

            foreach (var record in recordsToInsert)
            {
                var sqlParameters = new DynamicParameters();

                string recognitionNumberParameterName = "@recognitionNumber";
                string nameParameterName = "@employerAccountId";
                string legalNameParameterName = "@legalName";
                string acronymParameterName = "@acronym";
                string emailParameterName = "@email";
                string websiteParameterName = "@website";
                string headOfficeAddressLine1ParameterName = "@headOfficeAddressLine1";
                string headOfficeAddressLine2ParameterName = "@headOfficeAddressLine2";
                string headOfficeAddressTownParameterName = "@headOfficeAddressTown";
                string headOfficeAddressCountyParameterName = "@headOfficeAddressCounty";
                string headOfficeAddressPostcodeParameterName = "@headOfficeAddressPostcode";
                string headOfficeAddressCountryParameterName = "@headOfficeAddressCountry";
                string headOfficeAddressTelephoneParameterName = "@headOfficeAddressTelephone";
                string OfqualStatusParameterName = "@ofqualStatus";
                string OfqualRecognisedFromParameterName = "@ofqualRecognisedFrom";
                string OfqualRecognisedToParameterName = "@ofqualRecognisedTo";

                sqlParameters.Add(recognitionNumberParameterName, record.RecognitionNumber, DbType.String);
                sqlParameters.Add(nameParameterName, record.Name, DbType.String);
                sqlParameters.Add(legalNameParameterName, record.LegalName, DbType.String);
                sqlParameters.Add(acronymParameterName, record.Acronym, DbType.String);
                sqlParameters.Add(emailParameterName, record.Email, DbType.String);
                sqlParameters.Add(websiteParameterName, record.Website, DbType.String);
                sqlParameters.Add(headOfficeAddressLine1ParameterName, record.HeadOfficeAddressLine1, DbType.String);
                sqlParameters.Add(headOfficeAddressLine2ParameterName, record.HeadOfficeAddressLine2, DbType.String);
                sqlParameters.Add(headOfficeAddressTownParameterName, record.HeadOfficeAddressTown, DbType.String);
                sqlParameters.Add(headOfficeAddressCountyParameterName, record.HeadOfficeAddressCounty, DbType.String);
                sqlParameters.Add(headOfficeAddressPostcodeParameterName, record.HeadOfficeAddressPostcode, DbType.String);
                sqlParameters.Add(headOfficeAddressCountryParameterName, record.HeadOfficeAddressCountry, DbType.String);
                sqlParameters.Add(headOfficeAddressTelephoneParameterName, record.HeadOfficeAddressTelephone, DbType.String);
                sqlParameters.Add(OfqualStatusParameterName, record.OfqualStatus, DbType.String);
                sqlParameters.Add(OfqualRecognisedFromParameterName, record.OfqualRecognisedFrom, DbType.DateTime);
                sqlParameters.Add(OfqualRecognisedToParameterName, record.OfqualRecognisedTo, DbType.DateTime);

                string query = $"INSERT INTO [StagingOfqualOrganisation] VALUES ({recognitionNumberParameterName}, {nameParameterName}, {legalNameParameterName}, {acronymParameterName}, {emailParameterName}, {websiteParameterName}, {headOfficeAddressLine1ParameterName}, {headOfficeAddressLine2ParameterName}, {headOfficeAddressTownParameterName}, {headOfficeAddressCountyParameterName}, {headOfficeAddressPostcodeParameterName}, {headOfficeAddressCountryParameterName}, {headOfficeAddressTelephoneParameterName}, {OfqualStatusParameterName}, {OfqualRecognisedFromParameterName}, {OfqualRecognisedToParameterName})";
                
                recordsInserted += _connection.Execute(query, sqlParameters);
            }
            return recordsInserted;
        }

        private int InsertIntoOfqualQualificationStagingTable(IEnumerable<OfqualStandard> recordsToInsert)
        {
            int recordsInserted = 0;

            foreach (var record in recordsToInsert)
            {
                var sqlParameters = new DynamicParameters();

                string recognitionNumberParameterName = "@recognitionNumber";
                string operationalStartDateParameterName = "@operationalStartDate";
                string operationalEndDateParameterName = "@operationalEndDate";
                string ifateReferenceNumberParameterName = "@ifateReferenceNumber";

                sqlParameters.Add(recognitionNumberParameterName, record.RecognitionNumber, DbType.String);
                sqlParameters.Add(operationalStartDateParameterName, record.OperationalStartDate, DbType.DateTime);
                sqlParameters.Add(operationalEndDateParameterName, record.OperationalEndDate, DbType.DateTime);
                sqlParameters.Add(ifateReferenceNumberParameterName, record.IFateReferenceNumber, DbType.String);

                string query = $"INSERT INTO [StagingOfqualStandard] VALUES ({recognitionNumberParameterName}, {operationalStartDateParameterName}, {operationalEndDateParameterName}, {ifateReferenceNumberParameterName})";
                
                recordsInserted += _connection.Execute(query, sqlParameters);
            }
            return recordsInserted;
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

        public async Task<int> ClearStagingOfsOrganisationsTable()
        {
            return await _connection.ExecuteAsync("DELETE FROM [dbo].[StagingOfsOrganisation]");
        }

        public async Task<int> InsertIntoStagingOfsOrganisationTable(IEnumerable<OfsOrganisation> recordsToInsert)
        {
            int recordsInserted = 0;

            foreach (var record in recordsToInsert)
            {
                var sqlParameters = new DynamicParameters();

                string ukprnParameterName = "@ukprn";
                string registrationStatusParameterName = "@registrationStatus";
                string highestLevelOfDegreeAwardingPowersParameterName = "@highestLevelOfDegreeAwardingPowers";

                sqlParameters.Add(ukprnParameterName, record.Ukprn, DbType.Int32);
                sqlParameters.Add(registrationStatusParameterName, record.RegistrationStatus, DbType.String);
                sqlParameters.Add(highestLevelOfDegreeAwardingPowersParameterName, record.HighestLevelOfDegreeAwardingPowers, DbType.String);
                
                string query = $"INSERT INTO [StagingOfsOrganisation] VALUES ({ukprnParameterName}, {registrationStatusParameterName}, {highestLevelOfDegreeAwardingPowersParameterName})";

                recordsInserted += await _connection.ExecuteAsync(query, sqlParameters);
            }

            return recordsInserted;
        }

        public async Task<int> LoadOfsStandards()
        {
            var parameters = new DynamicParameters();
            parameters.Add("@inserted", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync("Load_Ofs_Standards", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@inserted");
        }

        public async Task<List<string>> DatabaseMaintenance()
        {
            var results = await _connection.QueryAsync<string>("DatabaseMaintenance",
                commandTimeout: 28800,
                commandType: CommandType.StoredProcedure);

            return results
                .ToList();
        }
    }
}