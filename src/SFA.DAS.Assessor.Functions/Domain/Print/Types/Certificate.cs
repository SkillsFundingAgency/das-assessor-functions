using System;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class Certificate
    {
        public string CertificateReference { get; set; }
        public int? BatchId { get; set; }
        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }
        public string  Reason { get; set; }
        public long Uln { get; set; }
        public int StandardCode { get; set; }
        public int ProviderUkPrn { get; set; }
        public string EndPointAssessorOrganisationId { get; set; }
        public string EndPointAssessorOrganisationName { get; set; }
        public string LearnerGivenNames { get; set; }
        public string LearnerFamilyName { get; set; }
        public string StandardReference { get; set; }
        public string StandardName { get; set; }
        public int StandardLevel { get; set; }
        public bool CoronationEmblem { get; set; }
        public DateTime? StandardPublicationDate { get; set; }
        public string ContactName { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactAddLine1 { get; set; }
        public string ContactAddLine2 { get; set; }
        public string ContactAddLine3 { get; set; }
        public string ContactAddLine4 { get; set; }
        public string ContactPostCode { get; set; }
        public string Registration { get; set; }
        public string ProviderName { get; set; }
        public DateTime LearningStartDate { get; set; }
        public DateTime? AchievementDate { get; set; }
        public string CourseOption { get; set; }
        public string OverallGrade { get; set; }
        public string Department { get; set; }
        public string FullName { get; set; }
        
        public static Certificate FromCertificatePrintSummary(CertificatePrintSummary summary)
        {
            return new Certificate
            {
                Uln = summary.Uln,
                StandardCode = summary.StandardCode,
                ProviderUkPrn = summary.ProviderUkPrn,
                EndPointAssessorOrganisationId = summary.EndPointAssessorOrganisationId,
                EndPointAssessorOrganisationName = summary.EndPointAssessorOrganisationName,
                CertificateReference = summary.CertificateReference,
                LearnerGivenNames = summary.LearnerGivenNames,
                LearnerFamilyName = summary.LearnerFamilyName,
                StandardName = summary.StandardName,
                StandardLevel = summary.StandardLevel,
                CoronationEmblem = summary.CoronationEmblem,
                ContactName = summary.ContactName,
                ContactOrganisation = summary.ContactOrganisation,
                ContactAddLine1 = summary.ContactAddLine1,
                ContactAddLine2 = summary.ContactAddLine2,
                ContactAddLine3 = summary.ContactAddLine3,
                ContactAddLine4 = summary.ContactAddLine4,
                ContactPostCode = summary.ContactPostCode,
                AchievementDate = summary.AchievementDate,
                CourseOption = summary.CourseOption,
                OverallGrade = summary.OverallGrade,
                Department = summary.Department,
                FullName = summary.FullName,
                Status = summary.Status
            };
        }
    }

}
