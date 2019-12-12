using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public class DataAccess : IDataAccess
    {

        public IEnumerable<dynamic> GetCurrentApplyApplicationSections(SqlConnection applyConnection, dynamic originalApplyApplication)
        {
            return applyConnection.Query("SELECT * FROM ApplicationSections WHERE ApplicationId = @ApplicationId",
                                                        new { ApplicationId = originalApplyApplication.Id });
        }

        public void CreateQnaApplicationSequencesRecord(SqlConnection qnaConnection, Guid qnaApplicationId, dynamic applySequence)
        {
            // Create Qna ApplicationSequences record
            qnaConnection.Execute("INSERT INTO ApplicationSequences (Id, ApplicationId, SequenceNo, IsActive) VALUES (@Id, @ApplicationId, @SequenceNo, @IsActive)",
                                    new { Id = applySequence.Id, ApplicationId = qnaApplicationId, SequenceNo = applySequence.SequenceId, IsActive = applySequence.IsActive });
        }

        public IEnumerable<dynamic> GetCurrentApplyApplicationSequences(SqlConnection applyConnection, dynamic originalApplyApplication)
        {
            return applyConnection.Query("SELECT * FROM ApplicationSequences WHERE ApplicationId = @ApplicationId", new { ApplicationId = originalApplyApplication.Id });
        }

        public Guid CreateQnaApplicationRecord(SqlConnection qnaConnection, Guid? workflowId, dynamic originalApplyApplication)
        {
            // Create Qna Applications record
            qnaConnection.Execute(@"INSERT INTO Applications (Id, WorkflowId, Reference, CreatedAt, ApplicationStatus, ApplicationData) 
                                            VALUES (@Id, @WorkflowId, 'Migrated from Apply', @CreatedAt, @ApplicationStatus, '')",
                                    new { Id = originalApplyApplication.Id, WorkflowId = workflowId, originalApplyApplication.CreatedAt, originalApplyApplication.ApplicationStatus });
            return originalApplyApplication.Id;
        }

        public void UpdateQnaApplicationData(SqlConnection qnaConnection, Guid applicationId, string applicationData)
        {
            qnaConnection.Execute("UPDATE Applications SET ApplicationData = @applicationData WHERE Id = @applicationId", new{applicationId, applicationData});
        }

        public List<dynamic> GetCurrentApplyApplications(SqlConnection applyConnection)
        {
            return applyConnection.Query(@"SELECT *, JSON_Value(ApplicationData, '$.StandardCode') AS StandardCode, 
				JSON_QUERY(FinancialSection.QnaData, '$.FinancialApplicationGrade') AS FinancialGrade,
				JSON_QUERY(FinancialSection.QnaData, '$.Pages[0].PageOfAnswers[0].Answers') AS FinancialAnswers,
				FinancialSection.Id AS FinancialSectionGuid,
				SequenceOne.Id AS SequenceOneGuid,
				Organisations.Name,
				Organisations.OrganisationDetails,
				Applications.Id AS OriginalApplicationId,
				FinancialStatus = FinancialSection.Status,
				SelectedGrade = json_value(JSON_QUERY(FinancialSection.QnaData, '$.FinancialApplicationGrade') ,'$.SelectedGrade'),
				SequenceOneIsActive = SequenceOne.IsActive,
				SequenceOneNotRequired = SequenceOne.NotRequired,
				SequenceOneStatus = SequenceOne.Status,
				ReviewStatus = SequenceOne.Status,
				FinancialExempt = (CASE WHEN FinancialSection.NotRequired = 1 THEN FinancialSection.NotRequired ELSE CAST(COALESCE(Json_value(OrganisationDetails, '$.FHADetails.FinancialExempt'), 'false') AS bit) END)
				FROM Applications 
				INNER JOIN ApplicationSections AS FinancialSection ON Applications.Id = FinancialSection.ApplicationId AND FinancialSection.SectionId = 3
				INNER JOIN ApplicationSequences AS SequenceOne ON Applications.Id = SequenceOne.ApplicationId AND SequenceOne.SequenceId = 1
				INNER JOIN Organisations ON Organisations.Id = Applications.ApplyingOrganisationId").ToList(); 
        }

        public Guid? GetExistingOrganisation(SqlConnection assessorConnection, dynamic applyingOrganisation)
        {
            return assessorConnection.QuerySingleOrDefault<Guid>("SELECT Id FROM Organisations WHERE EndPointAssessorUkprn = @ukprn", new { ukprn = applyingOrganisation.OrganisationUKPRN });
        }

        public void CreateQnaApplicationSectionsRecord(SqlConnection qnaConnection, Guid qnaApplicationId, dynamic applySequence, dynamic applySection)
        {
            qnaConnection.Execute(@"INSERT INTO ApplicationSections (Id, ApplicationId, SequenceNo, SectionNo, QnaData, Title, LinkTitle, DisplayType, SequenceId) 
                                                    VALUES (@Id, @ApplicationId, @SequenceNo, @SectionNo, @QnaData, @Title, @LinkTitle, @DisplayType, @SequenceId)",
                                                                new
                                                                {
                                                                    Id = applySection.Id,
                                                                    ApplicationId = qnaApplicationId,
                                                                    SequenceNo = applySequence.SequenceId,
                                                                    SectionNo = applySection.SectionId,
                                                                    QnaData = applySection.QnAData,
                                                                    Title = applySection.Title,
                                                                    LinkTitle = applySection.LinkTitle,
                                                                    DisplayType = applySection.DisplayType,
                                                                    SequenceId = applySequence.Id
                                                                });
        }

        public Guid CreateNewOrganisation(SqlConnection assessorConnection, dynamic originalApplyApplication, dynamic originalApplyOrganisation)
        {
            var orgDataObj = JObject.Parse(originalApplyApplication.OrganisationDetails);
            orgDataObj.Add("OriginalOrganisationId", originalApplyOrganisation.Id);
            var orgData = JsonConvert.SerializeObject(orgDataObj);
            
            string assessorName = ((string)originalApplyApplication.Name).Truncate(100);
            
            Guid? organisationId;

            organisationId = assessorConnection.QueryFirstOrDefault<Guid>("select Id FROM Organisations where EndPointAssessorName=@EndPointAssessorName", new {EndPointAssessorName = assessorName});
            if (organisationId != null)
            {
                return organisationId.Value;
            }            

            string nextEpaOrgId = GetNextEpaOrgId(assessorConnection);

            var organisationTypeId = assessorConnection.QuerySingle<int>("SELECT Id FROM OrganisationType WHERE REPLACE(Type,' ','') = @Type OR Type = @Type", 
                                                                        new {Type = originalApplyOrganisation.OrganisationType});

            organisationId = Guid.NewGuid();
            assessorConnection.Execute(@"INSERT INTO Organisations (Id, CreatedAt, EndPointAssessorName, EndPointAssessorOrganisationId, EndPointAssessorUkprn, PrimaryContact, Status, OrganisationData, ApiEnabled, OrganisationTypeId) 
                                                    VALUES (@Id, @CreatedAt, @EndPointAssessorName, @EndPointAssessorOrganisationId, @EndPointAssessorUkprn, @PrimaryContact, 'Applying', @OrganisationData, 0, @OrganisationTypeId)",
                                        new
                                        {
                                            Id = organisationId,
                                            CreatedAt = originalApplyApplication.CreatedAt,
                                            EndPointAssessorName = assessorName,
                                            EndPointAssessorOrganisationId = nextEpaOrgId,
                                            EndPointAssessorUkPrn = (string)null,
                                            PrimaryContact = (string)null,
                                            OrganisationData = orgData,
                                            OrganisationTypeId = organisationTypeId
                                        });
            return organisationId.Value;
        }

        private static string GetNextEpaOrgId(SqlConnection assessorConnection)
        {
            var sqlToGetHighestOrganisationId = @"select max(CAST( replace(EndPointAssessorOrganisationId,'EPA','') AS int)) OrgId from organisations where EndPointAssessorOrganisationId like 'EPA%' 
                                                        and isnumeric(replace(EndPointAssessorOrganisationId,'EPA','')) = 1";
            var highestEpaOrgId = assessorConnection.ExecuteScalar<string>(sqlToGetHighestOrganisationId);

            var nextEpaOrgId = int.TryParse(highestEpaOrgId.Replace("EPA", string.Empty), out int currentIntValue)
                ? $@"EPA{currentIntValue + 1:D4}" :
                string.Empty;
            return nextEpaOrgId;
        }

        public Guid? GetEpaoWorkflowId(SqlConnection qnaConnection)
        {
            return qnaConnection.QueryFirstOrDefault<Guid?>("SELECT Id FROM Workflows WHERE Type = 'EPAO' AND Status = 'Live'");
        }

        public dynamic GetApplyingOrganisation(SqlConnection applyConnection, Guid organisationId)
        {
            return applyConnection.QuerySingle("SELECT * FROM Organisations WHERE Id = @Id", new { Id = organisationId });
        }

        public void CreateAssessorApplyRecord(SqlConnection assessorConnection, dynamic originalApplyApplication, Guid qnaApplicationId, Guid? organisationId, dynamic applyDataObject, dynamic financialGradeObject, string financialReviewStatus, string applicationStatus, string reviewStatus)
        {
            assessorConnection.Execute(@"INSERT INTO Apply (Id, ApplicationId, OrganisationId, ApplicationStatus, ReviewStatus, ApplyData, FinancialReviewStatus, FinancialGrade, StandardCode, CreatedAt, CreatedBy) 
                                                    VALUES (NEWID(), @ApplicationId, @OrganisationId, @ApplicationStatus, @ReviewStatus, @ApplyData, @FinancialReviewStatus, @FinancialGrade, @StandardCode, @CreatedAt, @CreatedBy)", new
            {
                ApplicationId = qnaApplicationId,
                OrganisationId = organisationId.Value,
                ApplicationStatus = applicationStatus,
                ReviewStatus = reviewStatus,
                ApplyData = (string)applyDataObject,
                FinancialReviewStatus = financialReviewStatus,
                FinancialGrade = (string)financialGradeObject,
                StandardCode = (string)null,
                CreatedAt = originalApplyApplication.CreatedAt,
                CreatedBy = originalApplyApplication.CreatedBy
            });
        }

        public List<dynamic> GetApplyOrganisationContacts(SqlConnection applyConnection, Guid id)
        {
            return applyConnection.Query("SELECT * FROM Contacts WHERE ApplyOrganisationId = @OrganisationId", new {OrganisationId = id}).ToList();
        }

        public void CreateContact(SqlConnection assessorConnection, dynamic contact, Guid organisationId)
        {
            try {

            assessorConnection.Execute(@"INSERT INTO Contacts (Id, CreatedAt, DisplayName, Email, OrganisationId, Status, UpdatedAt, Username, GivenNames, FamilyName, SignInType, SignInId) 
                    VALUES (NEWID(), @CreatedAt, @DisplayName, @Email, @OrganisationId, 'Applying', GETUTCDATE(), @Email, @GivenNames, @FamilyName, 'AsLogin', @SignInId)", 
                new {
                    CreatedAt = contact.CreatedAt,
                    DisplayName = contact.GivenNames + " " + contact.FamilyName,
                    Email = contact.Email,
                    organisationId = organisationId,
                    GivenNames = contact.GivenNames,
                    FamilyName = contact.FamilyName,
                    SignInId = contact.SigninId
                });
            }
            catch(SqlException ex)
            {
                if (!ex.Message.Contains("UNIQUE KEY constraint"))
                {
                    throw ex;
                }                
            }
        }

        public int GetNextAppReferenceSequence(SqlConnection assessorConnection)
        {
            return (assessorConnection.Query<int>(@"SELECT NEXT VALUE FOR AppRefSequence")).FirstOrDefault();
        }
    }
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}