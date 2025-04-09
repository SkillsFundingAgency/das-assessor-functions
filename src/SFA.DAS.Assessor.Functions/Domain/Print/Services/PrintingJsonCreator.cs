using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class PrintingJsonCreator : IPrintCreator
    {
        private readonly CertificateDetails _certificateDetails;

        public PrintingJsonCreator(
            IOptions<CertificateDetails> options)
        {
            _certificateDetails = options?.Value;
        }

        public PrintOutput Create(int batchNumber, IEnumerable<CertificatePrintSummaryBase> certificates)
        {
            var printOutput = new PrintOutput
            {
                Batch = new BatchData()
                {
                    BatchNumber = batchNumber,
                    BatchDate = DateTime.UtcNow,
                    TotalCertificateCount = certificates.Count()
                },
                PrintData = new List<PrintData>()
            };

            var standardsPostalContactCount = AddStandardPrintOuput(printOutput, certificates.OfType<CertificatePrintSummary>());
            var frameworksPostalContactCount = AddFrameworkPrintOuput(printOutput, certificates.OfType<FrameworkCertificatePrintSummary>());

            printOutput.Batch.PostalContactCount = standardsPostalContactCount + frameworksPostalContactCount;

            return printOutput;
        }

        private int AddStandardPrintOuput(PrintOutput printOutput, IEnumerable<CertificatePrintSummary> certificates)
        {
            var groupedByRecipient = certificates.GroupBy(c =>
                new
                {
                    c.ContactName,
                    c.ContactOrganisation,
                    c.Department,
                    c.ContactAddLine1,
                    c.ContactAddLine2,
                    c.ContactAddLine3,
                    c.ContactAddLine4,
                    c.ContactPostCode
                }).ToList();

            groupedByRecipient.ForEach(g =>
            {
                var contactName = string.Empty;
                if (g.Key.ContactName != null)
                    contactName = g.Key.ContactName.Replace("\t", " ");

                var printData = new PrintData
                {
                    Type = "Standard",
                    PostalContact = new PostalContact
                    {
                        Name = contactName,
                        Department = g.Key.Department,
                        EmployerName = g.Key.ContactOrganisation,
                        AddressLine1 = g.Key.ContactAddLine1,
                        AddressLine2 = g.Key.ContactAddLine2,
                        AddressLine3 = g.Key.ContactAddLine3,
                        AddressLine4 = g.Key.ContactAddLine4,
                        Postcode = g.Key.ContactPostCode,
                        CertificateCount = g.Count()
                    },
                    CoverLetter = new CoverLetter
                    {
                        ChairName = _certificateDetails.ChairName,
                        ChairTitle = _certificateDetails.ChairTitle
                    },
                    Certificates = new List<PrintCertificate>()
                };

                g.ToList().ForEach(c =>
                {
                    var gradeText = string.Empty;
                    var grade = string.Empty;

                    if (!string.IsNullOrWhiteSpace(c.OverallGrade) && c.OverallGrade != "No grade awarded")
                    {
                        gradeText = "Achieved grade ";
                        grade = c.OverallGrade;
                    }

                    printData.Certificates.Add(new StandardPrintCertificate
                    {
                        CertificateNumber = c.CertificateReference,
                        ApprenticeName = $"{c.LearnerGivenNames} {c.LearnerFamilyName}",
                        LearningDetails = new StandardLearningDetails()
                        {
                            StandardTitle = c.StandardName,
                            Level = $"LEVEL {c.StandardLevel}",
                            Option = string.IsNullOrWhiteSpace(c.CourseOption) ? string.Empty : $"({c.CourseOption}):",
                            GradeText = gradeText,
                            Grade = grade,
                            AchievementDate = !c.AchievementDate.HasValue ? string.Empty : $"{c.AchievementDate.Value:dd MMMM yyyy}",
                            CoronationEmblem = c.CoronationEmblem
                        }
                    });
                });

                printOutput.PrintData.Add(printData);
            });

            return groupedByRecipient.Count;
        }

        private int AddFrameworkPrintOuput(PrintOutput printOutput, IEnumerable<FrameworkCertificatePrintSummary> certificates)
        {
            var groupedByRecipient = certificates.GroupBy(c =>
                new
                {
                    c.ContactName,
                    c.ContactAddLine1,
                    c.ContactAddLine2,
                    c.ContactAddLine3,
                    c.ContactAddLine4,
                    c.ContactPostCode,
                    Type = c.GetType()
                }).ToList();

            groupedByRecipient.ForEach(g =>
            {
                var contactName = string.Empty;
                if (g.Key.ContactName != null)
                    contactName = g.Key.ContactName.Replace("\t", " ");

                var printData = new PrintData
                {
                    Type = "Framework",
                    PostalContact = new PostalContact
                    {
                        Name = contactName,
                        AddressLine1 = g.Key.ContactAddLine1,
                        AddressLine2 = g.Key.ContactAddLine2,
                        AddressLine3 = g.Key.ContactAddLine3,
                        AddressLine4 = g.Key.ContactAddLine4,
                        Postcode = g.Key.ContactPostCode,
                        CertificateCount = g.Count()
                    },
                    CoverLetter = new CoverLetter
                    {
                        ChairName = _certificateDetails.FrameworksChairName,
                        ChairTitle = _certificateDetails.FrameworksChairTitle
                    },
                    Certificates = new List<PrintCertificate>()
                };

                g.ToList().ForEach(c =>
                {
                    printData.Certificates.Add(new FrameworkPrintCertificate
                    {
                        CertificateNumber = c.CertificateReference,
                        ApprenticeName = c.FullName,
                        LearningDetails = new FrameworkLearningDetails()
                        {
                            FrameworkName = c.FrameworkName,
                            PathwayName = !c.PathwayName.Equals(c.FrameworkName, StringComparison.OrdinalIgnoreCase) ? c.PathwayName : string.Empty,
                            LevelName = $"{c.FrameworkLevelName} Level",
                            FrameworkCertificateNumber = c.FrameworkCertificateNumber,
                            AchievementDate = !c.AchievementDate.HasValue ? string.Empty : $"{c.AchievementDate.Value:dd MMMM yyyy}",
                        }
                    });
                });

                printOutput.PrintData.Add(printData);
            });

            return groupedByRecipient.Count;
        }
    }
}
