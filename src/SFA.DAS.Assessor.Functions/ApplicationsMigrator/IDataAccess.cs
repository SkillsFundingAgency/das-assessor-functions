using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public interface IDataAccess
    {
        void CreateQnaApplicationSectionsRecord(SqlConnection qnaConnection, Guid qnaApplicationId, dynamic applySequence, dynamic applySection);
        Guid CreateNewOrganisation(SqlConnection assessorConnection, dynamic originalApplyApplication, dynamic originalApplyOrganisation);
        Guid? GetExistingOrganisation(SqlConnection assessorConnection, dynamic applyingOrganisation);
        List<dynamic> GetCurrentApplyApplications(SqlConnection applyConnection);
        IEnumerable<dynamic> GetCurrentApplyApplicationSections(SqlConnection applyConnection, dynamic originalApplyApplication);
        void CreateQnaApplicationSequencesRecord(SqlConnection qnaConnection, Guid qnaApplicationId, dynamic applySequence);
        IEnumerable<dynamic> GetCurrentApplyApplicationSequences(SqlConnection applyConnection, dynamic originalApplyApplication);
        Guid CreateQnaApplicationRecord(SqlConnection qnaConnection, Guid? workflowId, dynamic originalApplyApplication);
        void UpdateQnaApplicationData(SqlConnection qnaConnection, Guid applicationId, string applicationData);
        Guid? GetEpaoWorkflowId(SqlConnection qnaConnection);

        dynamic GetApplyingOrganisation(SqlConnection applyConnection, Guid organisationId);
        void CreateAssessorApplyRecord(SqlConnection assessorConnection, dynamic originalApplyApplication, Guid qnaApplicationId, Guid? organisationId, dynamic applyDataObject, dynamic financialGradeObject);
        List<dynamic> GetApplyOrganisationContacts(SqlConnection applyConnection, Guid id);
        void CreateContact(SqlConnection assessorConnection, dynamic contact, Guid organisationId);
        int GetNextAppReferenceSequence(SqlConnection assessorConnection);
    }
}