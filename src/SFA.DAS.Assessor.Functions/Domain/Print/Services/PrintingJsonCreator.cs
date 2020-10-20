using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class PrintingJsonCreator : IPrintingJsonCreator
    {
        private readonly CertificateDetails _certificateDetails;

        public PrintingJsonCreator(
            IOptions<CertificateDetails> options)
        {
            _certificateDetails = options?.Value;
        }

        public string Create(int batchNumber, IEnumerable<Certificate> certificates, string file)
        {
            var printOutput = new PrintOutput
            {
                Batch = new BatchData()
                {
                    BatchNumber = batchNumber,
                    BatchDate = DateTime.UtcNow
                },
                PrintData = new List<PrintData>()
            };

            printOutput.Batch.TotalCertificateCount = certificates.Count();

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

            printOutput.Batch.PostalContactCount = groupedByRecipient.Count;

            groupedByRecipient.ForEach(g =>
            {
                var contactName = string.Empty;
                if (g.Key.ContactName != null)
                    contactName = g.Key.ContactName.Replace("\t", " ");

                var printData = new PrintData
                {
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
                    var learnerName =
                        !string.IsNullOrEmpty(c.FullName)
                            ? c.FullName
                            : $"{c.LearnerGivenNames} {c.LearnerFamilyName}";

                    var gradeText = string.Empty;
                    var grade = string.Empty;

                    if (!string.IsNullOrWhiteSpace(c.OverallGrade) && c.OverallGrade != "No grade awarded")
                    {
                        gradeText = "Achieved grade ";
                        grade = c.OverallGrade;
                    }

                    printData.Certificates.Add(new PrintCertificate
                    {
                        CertificateNumber = c.CertificateReference,
                        ApprenticeName = $"{c.LearnerGivenNames.ProperCase()} {c.LearnerFamilyName.ProperCase(true)}",
                        LearningDetails = new LearningDetails()
                        {
                            StandardTitle = c.StandardName,
                            Level = $"LEVEL {c.StandardLevel}",
                            Option = string.IsNullOrWhiteSpace(c.CourseOption) ? string.Empty : $"({c.CourseOption}):",
                            GradeText = gradeText,
                            Grade = grade,
                            AchievementDate = !c.AchievementDate.HasValue ? string.Empty : $"{c.AchievementDate.Value:dd MMMM yyyy}"
                        }
                    });
                });

                printOutput.PrintData.Add(printData);
            });

            return JsonConvert.SerializeObject(printOutput);
        }
    }
}
