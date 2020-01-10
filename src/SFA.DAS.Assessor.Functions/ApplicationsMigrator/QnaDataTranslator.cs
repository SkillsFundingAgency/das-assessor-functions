using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.QnA.Api.Types.Page;

namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public class QnaDataTranslator : IQnaDataTranslator
    {
        public string Translate(dynamic applicationSection, dynamic applySequence, Microsoft.Extensions.Logging.ILogger log)
        {
            var qnaData = JsonConvert.DeserializeObject<QnAData>((string)applicationSection.QnAData);

            CreateNotRequiredConditions(qnaData);
            CreateActivatedByPageId(qnaData);
            ConvertNextConditionsToArray(qnaData);
            ConvertDateOfBirthToMonthAndYear(qnaData);
            UpdateActiveStatusses(qnaData);

            FixQuestionTags(qnaData);

            FixAddressData(qnaData);

            FixPageSequenceAndSectionIds(qnaData, applicationSection, applySequence, log);

            FixEmptyFileUploadAnswers(qnaData);
            MoveFileUploadAnswersIntoSeparateAnswerPages(qnaData);

            FixMissingComplexRadioAnswers(qnaData);

            string serializedQnaData = JsonConvert.SerializeObject(qnaData);
            return serializedQnaData;
        }

        private void MoveFileUploadAnswersIntoSeparateAnswerPages(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                if(page.Questions.Any(q => q.Input.Type == "FileUpload"))
                {
                    var fileUploadAnswers = page.PageOfAnswers.SelectMany(poa => poa.Answers).ToList();
                    page.PageOfAnswers.Clear();

                    foreach (var fileUploadAnswer in fileUploadAnswers)
                    {
                        page.PageOfAnswers.Add(new PageOfAnswers{Id = Guid.NewGuid(), Answers = new List<Answer>{fileUploadAnswer}});
                    }
                }
            }
        }

        private void FixMissingComplexRadioAnswers(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                if (page.PageOfAnswers != null && page.PageOfAnswers.Any())
                {
                    foreach(var complexQuestion in page.Questions.Where(q => q.Input.Type == "ComplexRadio"))
                    {
                        foreach (var option in complexQuestion.Input.Options)
                        {
                            if (option.FurtherQuestions != null && option.FurtherQuestions.Any())
                            {
                                foreach (var furtherQuestion in option.FurtherQuestions)
                                {
                                    var answer = page.PageOfAnswers[0].Answers.SingleOrDefault(a => a.QuestionId == furtherQuestion.QuestionId);
                                    if (answer == null)
                                    {
                                        page.PageOfAnswers[0].Answers.Add(new Answer
                                        {
                                            QuestionId = furtherQuestion.QuestionId,
                                            Value = ""
                                        });
                                    }
                                }
                            }
                        }
                    }   
                }
            }
        }

        private void FixEmptyFileUploadAnswers(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                if(page.Questions.Any(q => q.Input.Type == "FileUpload"))
                foreach(var poa in page.PageOfAnswers)
                {
                    for (int i = 0; i < poa.Answers.Count(); i++)
                    {
                        if(string.IsNullOrWhiteSpace(poa.Answers[i].Value.ToString()))
                        {
                            poa.Answers.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void FixPageSequenceAndSectionIds(QnAData qnaData, dynamic applicationSection, dynamic applySequence, Microsoft.Extensions.Logging.ILogger log)
        {
            foreach (var page in qnaData.Pages)
            {
                page.SectionId = ((Guid)applicationSection.Id).ToString();
                page.SequenceId = ((Guid)applySequence.Id).ToString();
            }
        }

        private void FixAddressData(QnAData qnaData)
        {
            foreach (var page in qnaData.Pages)
            {
                foreach(var poa in page.PageOfAnswers)
                {
                    foreach (var answer in poa.Answers)
                    {
                        if(!(answer.Value is string))
                        {
                            answer.Value = JsonConvert.SerializeObject(answer.Value);
                        }
                    }
                }
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
                        furtherQuestion.QuestionTag = question.QuestionTag.Replace("-", "_");
                        question.QuestionTag = null;
                    }

                    if (question.QuestionId == "CD-26")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-26.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag.Replace("-", "_");
                        question.QuestionTag = null;
                    }

                    if (question.QuestionId == "CD-12")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-12.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag.Replace("-", "_");
                        question.QuestionTag = null;
                    }

                    if (question.QuestionId == "CD-17")
                    {
                        var furtherQuestion = question.Input.Options.First(o => o.FurtherQuestions.First().QuestionId == "CD-17.1").FurtherQuestions.First();
                        furtherQuestion.QuestionTag = question.QuestionTag.Replace("-", "_");
                        question.QuestionTag = null;
                    }

                    if (question.QuestionTag != null)
                    {
                        question.QuestionTag = question.QuestionTag.Replace("-", "_");
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