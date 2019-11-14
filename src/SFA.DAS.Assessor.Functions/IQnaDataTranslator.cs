using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using SFA.DAS.QnA.Api.Types.Page;

namespace SFA.DAS.Assessor.Functions
{
    public interface IQnaDataTranslator
    {
        void Translate(System.Data.SqlClient.SqlConnection qnaConnection);
    }

    public class QnaDataTranslator : IQnaDataTranslator
    {
        public void Translate(SqlConnection qnaConnection)
        {
            var qnaSections = qnaConnection.Query("SELECT * FROM ApplicationSections");
            foreach (var applicationSection in qnaSections)
            {
                var qnaData = JsonConvert.DeserializeObject<QnAData>((string)applicationSection.QnAData);

                CreateNotRequiredConditions(qnaData);
                CreateActivatedByPageId(qnaData);
                ConvertNextConditionsToArray(qnaData);
                ConvertDateOfBirthToMonthAndYear(qnaData);
                UpdateActiveStatusses(qnaData);

                FixQuestionTags(qnaData);

                qnaConnection.Execute("UPDATE ApplicationSections SET QnAData = @qnaData WHERE Id = @id", new { qnaData = JsonConvert.SerializeObject(qnaData), id = applicationSection.Id });
            }
        }

        private void FixQuestionTags(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                foreach(var question in page.Questions)
                {
                    if (question.QuestionId == "CD-30")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-30.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag;
                        question.QuestionTag = null;
                    }

                    if (question.QuestionId == "CD-26")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-26.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag;
                        question.QuestionTag = null;
                    }

                    if (question.QuestionId == "CD-12")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-12.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag;
                        question.QuestionTag = null;
                    }

                    if (question.QuestionId == "CD-17")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-17.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag;
                        question.QuestionTag = null;
                    }
                }
            }
        }

        private static void CreateNotRequiredConditions(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                var notRequiredOrgTypes = page.NotRequiredOrgTypes;

                if(notRequiredOrgTypes != null)
                {
                    page.NotRequiredConditions = new List<NotRequiredCondition>();

                    if (notRequiredOrgTypes.Length > 0)
                    {
                        page.NotRequiredConditions.Add(new NotRequiredCondition { Field = "OrganisationType", IsOneOf = notRequiredOrgTypes });
                    }

                    page.NotRequiredOrgTypes = null;
                }
            }
        }

        private static void CreateActivatedByPageId(QnAData qnaData)
        {
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
        }

        private void ConvertNextConditionsToArray(QnAData qnaData)
        {
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
        }

        private void ConvertDateOfBirthToMonthAndYear(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                foreach (var question in page.Questions)
                {
                    if (question.Input.Type == "DateOfBirth")
                    {
                        question.Input.Type = "MonthAndYear";
                        foreach (var validation in question.Input.Validations)
                        {
                            validation.Name = validation.Name.Replace("DateOfBirth", "MonthAndYear");
                        }
                    }
                }
            }
        }

        private void UpdateActiveStatusses(QnAData qnaData)
        {
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
        }
    }
}