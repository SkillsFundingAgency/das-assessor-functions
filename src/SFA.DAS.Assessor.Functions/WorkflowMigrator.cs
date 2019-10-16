using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Linq;
using Newtonsoft.Json;
using SFA.DAS.QnA.Api.Types.Page;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.WorkflowMigrator
{
    public class WorkflowMigrator
    {
        private readonly SqlConnectionStrings _connectionStrings;

        private const string _schema = "{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"id\":\"http://example.com/example.json\",\"title\":\"ApplicationData\",\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"OrganisationReferenceId\",\"OrganisationName\"],\"properties\":{\"OrganisationReferenceId\":{\"type\":\"string\",\"minLength\":1},\"OrganisationName\":{\"type\":\"string\",\"minLength\":1},\"OrganisationType\":{\"anyOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"ReferenceNumber\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"StandardName\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"StandardCode\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"TradingName\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"UseTradingName\":{\"minLength\":1,\"type\":\"boolean\"},\"ContactGivenName\":{\"minLength\":1},\"CompanySummary\":{\"oneOf\":[{\"type\":\"null\"},{\"$ref\":\"#/definitions/CompaniesHouseSummary\"}]},\"CharitySummary\":{\"oneOf\":[{\"type\":\"null\"},{\"$ref\":\"#/definitions/CharityCommissionSummary\"}]}},\"definitions\":{\"CompaniesHouseSummary\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"CompanyName\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"CompanyNumber\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"Status\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"CompanyType\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"CompanyTypeDescription\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"IncorporationDate\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}],\"format\":\"date-time\"},\"Directors\":{\"oneOf\":[{\"type\":\"array\"},{\"type\":\"null\"}],\"items\":{\"$ref\":\"#/definitions/DirectorInformation\"}},\"PersonsWithSignificantControl\":{\"oneOf\":[{\"type\":\"array\"},{\"type\":\"null\"}],\"items\":{\"$ref\":\"#/definitions/PersonWithSignificantControlInformation\"}},\"ManualEntryRequired\":{\"minLength\":1,\"type\":\"boolean\"}}},\"DirectorInformation\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"Id\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"Name\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"DateOfBirth\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"AppointedDate\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}],\"format\":\"date-time\"},\"ResignedDate\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}],\"format\":\"date-time\"},\"Active\":{\"minLength\":1,\"type\":\"boolean\"}}},\"PersonWithSignificantControlInformation\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"Id\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"Name\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"DateOfBirth\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"NotifiedDate\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}],\"format\":\"date-time\"},\"CeasedDate\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}],\"format\":\"date-time\"},\"Active\":{\"minLength\":1,\"type\":\"boolean\"}}},\"CharityCommissionSummary\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"CharityName\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"CharityNumber\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"IncorporatedOn\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}],\"format\":\"date-time\"},\"Trustees\":{\"oneOf\":[{\"type\":\"array\"},{\"type\":\"null\"}],\"items\":{\"$ref\":\"#/definitions/TrusteeInformation\"}},\"TrusteeManualEntryRequired\":{\"minLength\":1,\"type\":\"boolean\"}}},\"TrusteeInformation\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"Id\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]},\"Name\":{\"oneOf\":[{\"type\":\"string\"},{\"type\":\"null\"}]}}}}}";

        public WorkflowMigrator(IOptions<SqlConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        [FunctionName("WorkflowMigrator")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"WorkflowMigrator - HTTP trigger function executed at: {DateTime.Now}");

            try
            {
                using (var applyConnection = new SqlConnection(_connectionStrings.Apply))
                using (var qnaConnection = new SqlConnection(_connectionStrings.QnA))
                {
                    var projectId = CreateProject(qnaConnection);
                    CreateWorkflows(applyConnection, qnaConnection, projectId);
                    MergeAssets(applyConnection, qnaConnection, projectId);
                    CreateNotRequiredConditions(qnaConnection, projectId);
                    CreateActivatedByPageId(qnaConnection, projectId);
                    ConvertNextConditionsToArray(qnaConnection, projectId);
                    ConvertDateOfBirthToMonthAndYear(qnaConnection, projectId);
                    UpdateActiveStatusses(qnaConnection, projectId);
                }
            }
            catch (Exception ex)
            { 
                return (ActionResult)new OkObjectResult($"Error: {ex.Message}, Stack: {ex.StackTrace}");
            }

            return (ActionResult)new OkObjectResult("Ok");
        }

        private void UpdateActiveStatusses(SqlConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new { projectId });
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string) workflowSection.QnAData);
                foreach (var page in qnaData.Pages)
                {
                    if (page.SectionId == "1" && page.SequenceId == "1" && page.PageId == "9")
                    {
                        page.Active = false;
                        page.ActivatedByPageId = "8";
                    }
                    
                    else if (page.SectionId == "1" && page.SequenceId == "1" && page.PageId == "10")
                    {
                        page.Active = false;
                        page.ActivatedByPageId = "9";
                    }
                }
                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new { qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id });
            }
        }

        private void ConvertDateOfBirthToMonthAndYear(SqlConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new { projectId });
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string)workflowSection.QnAData);
                foreach (var page in qnaData.Pages)
                {
                    foreach (var question in page.Questions)
                    {
                        if(question.Input.Type == "DateOfBirth")
                        {
                            question.Input.Type = "MonthAndYear";
                            foreach(var validation in question.Input.Validations)
                            {
                                validation.Name = validation.Name.Replace("DateOfBirth", "MonthAndYear");
                            }
                        }
                    }
                }
                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new { qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id });
            }
        }

        private static Guid CreateProject(IDbConnection qnaConnection)
        {
            var projectId = Guid.NewGuid();
            qnaConnection.Execute(@"INSERT INTO Projects (Id, Name, Description, ApplicationDataSchema, CreatedAt, CreatedBy) 
                                            VALUES (@projectId, 'EPAO Project', 'EPAO', @applicationDataSchema, @createdAt, 'Migration')",
                new
                {
                    projectId,
                    applicationDataSchema = _schema,
                    createdAt = DateTime.UtcNow
                });
            return projectId;
        }

        private static void CreateWorkflows(IDbConnection applyConnection, IDbConnection qnaConnection, Guid projectId)
        {
            var atWorkflows = applyConnection.Query("SELECT * FROM Workflows");
            foreach (var atWorkflow in atWorkflows)
            {
                qnaConnection.Execute(@"INSERT INTO Workflows (Id, Description, Version, Type, Status, CreatedAt, CreatedBy, ProjectId, ApplicationDataSchema) 
                                                            VALUES (@id, @description, @version, @type, @status, @createdAt, @createdBy, @projectId, @applicationDataSchema)",
                    new
                    {
                        atWorkflow.Id,
                        atWorkflow.Description,
                        atWorkflow.Version,
                        atWorkflow.Type,
                        atWorkflow.Status,
                        createdAt = DateTime.UtcNow,
                        createdBy = "Migration",
                        projectId,
                        applicationDataSchema = _schema
                    });

                var atSections = applyConnection.Query("SELECT * FROM WorkflowSections WHERE WorkflowId = @workflowId", new { workflowId = atWorkflow.Id });

                foreach (var atSection in atSections)
                {
                    qnaConnection.Execute(@"INSERT INTO WorkflowSequences (Id, WorkflowId, SequenceNo, SectionNo, SectionId, Status, IsActive) 
                    VALUES (@id, @workflowId, @sequenceNo, @sectionNo, @sectionId, 'Draft', 1)", new { id = Guid.NewGuid(), workflowId = atWorkflow.Id, sequenceNo = atSection.SequenceId, sectionNo = atSection.SectionId, sectionId = atSection.Id });

                    qnaConnection.Execute(@"INSERT INTO WorkflowSections (Id, ProjectId, QnaData, Title, LinkTitle, Status, DisplayType) 
                                                VALUES (@id, @projectId, @qnaData, @title, @linkTitle, 'Draft', @displayType)", new { id = atSection.Id, projectId, qnaData = atSection.QnAData, title = atSection.Title, linkTitle = atSection.LinkTitle, displayType = atSection.DisplayType });
                }
            }
        }

        private static void MergeAssets(IDbConnection applyConnection, IDbConnection qnaConnection, Guid projectId)
        {
            var assets = applyConnection.Query("SELECT * FROM Assets").ToList();
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new { projectId });
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = (string)workflowSection.QnAData;
                foreach (var asset in assets)
                {
                    qnaData = qnaData.Replace(asset.Reference, ((string)asset.Text).Replace("\"", "'"));
                }

                qnaConnection.Execute("UPDATE WorkflowSections SET QnaData = @qnaData WHERE Id = @id", new
                {
                    qnaData,
                    id = workflowSection.Id
                });
            }
        }

        private static void CreateNotRequiredConditions(SqlConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new { projectId });
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string)workflowSection.QnAData);
                foreach (var page in qnaData.Pages)
                {
                    var notRequiredOrgTypes = page.NotRequiredOrgTypes;

                    page.NotRequiredConditions = new List<NotRequiredCondition>();

                    if (notRequiredOrgTypes.Length > 0)
                    {
                        page.NotRequiredConditions.Add(new NotRequiredCondition { Field = "OrganisationType", IsOneOf = notRequiredOrgTypes });
                    }

                    page.NotRequiredOrgTypes = null;
                }

                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new { qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id });
            }
        }

        private static void CreateActivatedByPageId(IDbConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new { projectId });
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string)workflowSection.QnAData);
                for (int pageIndex = 0; pageIndex < qnaData.Pages.Count; pageIndex++)
                {
                    var currentPage = qnaData.Pages[pageIndex];
                    var nextPageActions = currentPage.Next.Where(n => n.Action == "NextPage").ToList();

                    if (nextPageActions.Count > 1)
                    {
                        foreach (var nextPageAction in nextPageActions)
                        {
                            var dependentPage = qnaData.Pages.SingleOrDefault(p => p.PageId == nextPageAction.ReturnId && p.Active == false);
                            if (dependentPage != null)
                            {
                                qnaData.Pages.Single(p => p.PageId == nextPageAction.ReturnId).ActivatedByPageId = currentPage.PageId;
                            }
                        }
                    }
                }
                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new { qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id });
            }
        }

        private void ConvertNextConditionsToArray(SqlConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new { projectId });
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string)workflowSection.QnAData);
                foreach (var page in qnaData.Pages)
                {
                    foreach (var next in page.Next)
                    {
                        next.Conditions = new List<Condition>();
                        if (next.Condition != null)
                        {
                            next.Conditions.Add(new Condition { QuestionId = next.Condition.QuestionId, MustEqual = next.Condition.MustEqual, QuestionTag = next.Condition.QuestionTag });
                            next.Condition = null;
                        }
                    }
                }
                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new { qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id });
            }
        }
    }
}