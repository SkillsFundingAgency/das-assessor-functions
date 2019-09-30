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
        private readonly IOptions<SqlConnectionStrings> _configuration;
        private readonly SqlConnectionStrings _connectionStrings;

        public WorkflowMigrator(IOptions<SqlConnectionStrings> configuration, IOptions<SqlConnectionStrings> connectionStrings)
        {
            _configuration = configuration;
            _connectionStrings = connectionStrings.Value;
        }

        [FunctionName("WorkflowMigrator")]
        public IActionResult Run( [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workflowMigrator")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"WorkflowMigrator - HTTP trigger function executed at: {DateTime.Now}");

            using (var applyConnection = new SqlConnection(_connectionStrings.Apply))
            using (var qnaConnection = new SqlConnection(_connectionStrings.QnA))
            {
                // Convert tables over to new format.
                var projectId = CreateProject(qnaConnection);
                CreateWorkflows(applyConnection, qnaConnection, projectId);
                // Merge assets into QnaData
                MergeAssets(applyConnection, qnaConnection, projectId);

                CreateNotRequiredConditions(qnaConnection, projectId);

                CreateActivatedByPageId(qnaConnection, projectId);

                ConvertNextConditionsToArray(qnaConnection, projectId);
            }

            return (ActionResult) new OkObjectResult("Ok");
        }

        private static Guid CreateProject(IDbConnection qnaConnection)
        {
            var projectId = Guid.NewGuid();
            qnaConnection.Execute(@"INSERT INTO Projects (Id, Name, Description, ApplicationDataSchema, CreatedAt, CreatedBy) 
                                            VALUES (@projectId, 'EPAO Project', 'EPAO', @applicationDataSchema, @createdAt, 'Migration')",
                new
                {
                    projectId, applicationDataSchema = "{   '$schema': 'http://json-schema.org/draft-04/schema#',   'definitions': {},   'id': 'http://example.com/example.json',   'properties': {     'TradingName': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'UseTradingName': {       'minLength': 1,       'type': 'boolean'     },     'ContactGivenName': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'ReferenceNumber': {        'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'StandardCode': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },      'StandardName': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'OrganisationReferenceId': {       'minLength': 1,       'type': 'string'     },     'OrganisationName': {       'minLength': 1,       'type': 'string'     }   },   'additionalProperties': false,   'required': [     'OrganisationReferenceId',     'OrganisationName'   ],   'type': 'object'  }",
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
                    new {atWorkflow.Id, atWorkflow.Description, atWorkflow.Version, atWorkflow.Type, atWorkflow.Status, createdAt = DateTime.UtcNow, createdBy = "Migration", projectId, applicationDataSchema = "{   '$schema': 'http://json-schema.org/draft-04/schema#',   'definitions': {},   'id': 'http://example.com/example.json',   'properties': {     'TradingName': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'UseTradingName': {       'minLength': 1,       'type': 'boolean'     },     'ContactGivenName': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'ReferenceNumber': {        'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'StandardCode': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },      'StandardName': {       'anyOf': [             {'type':'string'},             {'type':'null'}         ]     },     'OrganisationReferenceId': {       'minLength': 1,       'type': 'string'     },     'OrganisationName': {       'minLength': 1,       'type': 'string'     }   },   'additionalProperties': false,   'required': [     'OrganisationReferenceId',     'OrganisationName'   ],   'type': 'object'  }"});

                var atSections = applyConnection.Query("SELECT * FROM WorkflowSections WHERE WorkflowId = @workflowId", new { workflowId = atWorkflow.Id });

                foreach (var atSection in atSections)
                {
                    qnaConnection.Execute(@"INSERT INTO WorkflowSequences (Id, WorkflowId, SequenceNo, SectionNo, SectionId, Status, IsActive) 
                    VALUES (@id, @workflowId, @sequenceNo, @sectionNo, @sectionId, 'Draft', 1)", new {id = Guid.NewGuid(), workflowId = atWorkflow.Id, sequenceNo = atSection.SequenceId, sectionNo = atSection.SectionId, sectionId = atSection.Id});

                    qnaConnection.Execute(@"INSERT INTO WorkflowSections (Id, ProjectId, QnaData, Title, LinkTitle, Status, DisplayType) 
                                                VALUES (@id, @projectId, @qnaData, @title, @linkTitle, 'Draft', @displayType)", new {id = atSection.Id, projectId, qnaData = atSection.QnAData, title = atSection.Title, linkTitle = atSection.LinkTitle, displayType = atSection.DisplayType});
                }
            }
        }

        private static void MergeAssets(IDbConnection applyConnection, IDbConnection qnaConnection, Guid projectId)
        {
            var assets = applyConnection.Query("SELECT * FROM Assets").ToList();
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new {projectId});
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
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new {projectId});
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string)workflowSection.QnAData);
                foreach (var page in qnaData.Pages)
                {
                    var notRequiredOrgTypes = page.NotRequiredOrgTypes;
                    
                    page.NotRequiredConditions = new List<NotRequiredCondition>();
                    
                    if (notRequiredOrgTypes.Length > 0)
                    {
                        page.NotRequiredConditions.Add(new NotRequiredCondition {Field = "OrganisationType", IsOneOf = notRequiredOrgTypes});
                    }

                    page.NotRequiredOrgTypes = null;
                }

                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new {qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id});
            }
        }

        private static void CreateActivatedByPageId(IDbConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new {projectId});
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string) workflowSection.QnAData);
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
                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new {qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id});
            }
        }

        private void ConvertNextConditionsToArray(SqlConnection qnaConnection, Guid projectId)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM WorkflowSections WHERE ProjectId = @projectId", new {projectId});
            foreach (var workflowSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string) workflowSection.QnAData);
                foreach (var page in qnaData.Pages)
                {
                    foreach(var next in page.Next)
                    {
                        next.Conditions = new List<Condition>();
                        if(next.Condition != null)
                        {
                            next.Conditions.Add(new Condition{ QuestionId = next.Condition.QuestionId, MustEqual = next.Condition.MustEqual, QuestionTag = next.Condition.QuestionTag});
                            next.Condition = null;
                        }
                    }
                }
                qnaConnection.Execute("UPDATE WorkflowSections SET QnAData = @qnaData WHERE Id = @id", new {qnaData = JsonConvert.SerializeObject(qnaData), id = workflowSection.Id});
            }            
        }
    }
}