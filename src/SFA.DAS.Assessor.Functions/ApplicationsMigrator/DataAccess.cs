using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

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
            var qnaApplicationId = Guid.NewGuid();

            // Create Qna Applications record
            qnaConnection.Execute(@"INSERT INTO Applications (Id, WorkflowId, Reference, CreatedAt, ApplicationStatus, ApplicationData) 
                                            VALUES (@Id, @WorkflowId, 'Migrated from Apply', @CreatedAt, @ApplicationStatus, '')",
                                    new { Id = qnaApplicationId, WorkflowId = workflowId, originalApplyApplication.CreatedAt, originalApplyApplication.ApplicationStatus });
            return qnaApplicationId;
        }



        public List<dynamic> GetCurrentApplyApplications(SqlConnection applyConnection)
        {
            return applyConnection.Query(@"SELECT *, JSON_Value(ApplicationData, '$.StandardCode') AS StandardCode, 
                                                                JSON_QUERY(ApplicationSections.QnaData, '$.FinancialApplicationGrade') AS FinancialGrade,
                                                                JSON_QUERY(ApplicationSections.QnaData, '$.Pages[0].PageOfAnswers[0].Answers') AS FinancialAnswers,
																ApplicationSections.Id AS SectionGuid,
																ApplicationSequences.Id AS SequenceGuid,
																Organisations.Name,
																Organisations.OrganisationDetails,
																Applications.Id AS OriginalApplicationId
                                                                FROM Applications 
                                                                INNER JOIN ApplicationSections ON Applications.Id = ApplicationSections.ApplicationId
																INNER JOIN ApplicationSequences ON Applications.Id = ApplicationSequences.ApplicationId AND ApplicationSequences.SequenceId = 2
																INNER JOIN Organisations ON Organisations.Id = Applications.ApplyingOrganisationId
                                                                WHERE ApplicationStatus NOT IN ('Approved','Rejected') AND ApplicationSections.SectionId = 3").ToList();
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

        public Guid CreateNewOrganisation(SqlConnection assessorConnection, dynamic originalApplyApplication)
        {
            Guid organisationId;
            string nextEpaOrgId = GetNextEpaOrgId(assessorConnection);

            organisationId = Guid.NewGuid();
            assessorConnection.Execute(@"INSERT INTO Organisations (Id, CreatedAt, EndPointAssessorName, EndPointAssessorOrganisationId, EndPointAssessorUkprn, PrimaryContact, Status, OrganisationData, ApiEnabled) 
                                                    VALUES (@Id, @CreatedAt, @EndPointAssessorName, @EndPointAssessorOrganisationId, @EndPointAssessorUkprn, @PrimaryContact, 'Applying', @OrganisationData, 0)",
                                        new
                                        {
                                            Id = organisationId,
                                            CreatedAt = originalApplyApplication.CreatedAt,
                                            EndPointAssessorName = ((string)originalApplyApplication.Name).Truncate(100),
                                            EndPointAssessorOrganisationId = nextEpaOrgId,
                                            EndPointAssessorUkPrn = "",
                                            PrimaryContact = "",
                                            OrganisationData = originalApplyApplication.OrganisationDetails
                                        });
            return organisationId;
        }

        private static string GetNextEpaOrgId(SqlConnection assessorConnection)
        {
            var sqlToGetHighestOrganisationId = "select max(EndPointAssessorOrganisationId) OrgId from organisations where EndPointAssessorOrganisationId like 'EPA%' " +
                                                            " and isnumeric(replace(EndPointAssessorOrganisationId,'EPA','')) = 1";
            var highestEpaOrgId = assessorConnection.ExecuteScalar<string>(sqlToGetHighestOrganisationId);

            var nextEpaOrgId = int.TryParse(highestEpaOrgId.Replace("EPA", string.Empty), out int currentIntValue)
                ? $@"EPA{currentIntValue + 1:D4}" :
                string.Empty;
            return nextEpaOrgId;
        }

        public Guid? GetEpaoWorkflowId(SqlConnection qnaConnection)
        {
            return qnaConnection.QuerySingleOrDefault<Guid?>("SELECT Id FROM Workflows WHERE Type = 'EPAO'");
        }

        public dynamic GetApplyingOrganisation(SqlConnection applyConnection, Guid organisationId)
        {
            return applyConnection.QuerySingle("SELECT * FROM Organisations WHERE Id = @Id", new { Id = organisationId });
        }

        public void CreateAssessorApplyRecord(SqlConnection assessorConnection, dynamic originalApplyApplication, Guid qnaApplicationId, Guid? organisationId, dynamic applyDataObject, dynamic financialGradeObject)
        {
            assessorConnection.Execute(@"INSERT INTO Apply (Id, ApplicationId, OrganisationId, ApplicationStatus, ReviewStatus, ApplyData, FinancialReviewStatus, FinancialGrade, StandardCode, CreatedAt, CreatedBy) 
                                                    VALUES (NEWID(), @ApplicationId, @OrganisationId, @ApplicationStatus, @ReviewStatus, @ApplyData, @FinancialReviewStatus, @FinancialGrade, @StandardCode, @CreatedAt, @CreatedBy)", new
            {
                ApplicationId = qnaApplicationId,
                OrganisationId = organisationId.Value,
                ApplicationStatus = originalApplyApplication.ApplicationStatus,
                ReviewStatus = "", //TODO: ReviewStatus
                ApplyData = (string)applyDataObject,
                FinancialReviewStatus = "", // TODO: FinancialReviewStatus
                FinancialGrade = (string)financialGradeObject,
                StandardCode = "",
                CreatedAt = originalApplyApplication.CreatedAt,
                CreatedBy = originalApplyApplication.CreatedBy
            });
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