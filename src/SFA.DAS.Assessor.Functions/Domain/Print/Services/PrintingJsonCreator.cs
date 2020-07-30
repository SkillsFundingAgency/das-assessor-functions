﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using Microsoft.Extensions.Options;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class PrintingJsonCreator : IPrintingJsonCreator
    {
        private readonly ILogger<PrintingJsonCreator> _logger;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly CertificateDetails _certificateDetails;

        public PrintingJsonCreator(
            ILogger<PrintingJsonCreator> logger,
            IFileTransferClient fileTransferClient,
            IOptions<CertificateDetails> options)
        {
            _logger = logger;
            _fileTransferClient = fileTransferClient;
            _certificateDetails = options?.Value;
        }

        public void Create(int batchNumber, IEnumerable<Certificate> certificates, string file)
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
                            AchievementDate = $"{c.AchievementDate.Value:dd MMMM yyyy}",
                        }
                    });
                });

                printOutput.PrintData.Add(printData);
            });

            _logger.Log(LogLevel.Information, "Completed Certificates to print Json ....");
            var serializedPrintOutput = JsonConvert.SerializeObject(printOutput);
            byte[] array = Encoding.ASCII.GetBytes(serializedPrintOutput);
            using (var mystream = new MemoryStream(array))
            {
                _logger.Log(LogLevel.Information, "Sending Certificates to print Json ....");
                _fileTransferClient.Send(mystream, file);
            }

            // update certificates status
            foreach(var certificate in certificates)
            {
                certificate.Status = "SentToPrinter";
            }
        }
    }
}
