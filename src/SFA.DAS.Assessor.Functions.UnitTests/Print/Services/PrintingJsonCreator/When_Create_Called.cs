using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Services.PrintingJsonCreator
{
    public class When_Create_Called
    {
        private Domain.Print.Services.PrintingJsonCreator _sut;
        private Mock<IOptions<CertificateDetails>> _mockOptions;

        [SetUp]
        public void Setup()
        {
            _mockOptions = new Mock<IOptions<CertificateDetails>>();
            _mockOptions.Setup(o => o.Value).Returns(new CertificateDetails
            {
                ChairName = "Jane Smith",
                ChairTitle = "CEO",
                FrameworksChairName = "John Doe",
                FrameworksChairTitle = "Director"
            });

            _sut = new Domain.Print.Services.PrintingJsonCreator(_mockOptions.Object);
        }

        [Test]
        public void Create_Should_Return_One_Standard_PrintData_For_Single_Certificate()
        {
            var certificates = new List<CertificatePrintSummaryBase>
            {
                new CertificatePrintSummary
                {
                    ContactName = "Alice",
                    ContactOrganisation = "Org A",
                    ContactAddLine1 = "Address 1",
                    ContactPostCode = "AA1 1AA",
                    LearnerGivenNames = "Alice",
                    LearnerFamilyName = "Anderson",
                    StandardName = "Standard A",
                    StandardLevel = 2,
                    OverallGrade = "Pass",
                    AchievementDate = DateTime.UtcNow
                }
            };

            var result = _sut.Create(1, certificates);

            result.PrintData.Should().HaveCount(1);

            var certificatePrintSummary = certificates[0] as CertificatePrintSummary;

            result.PrintData[0].Type.Should().Be("Standard");
            result.PrintData[0].PostalContact.Should().BeEquivalentTo(CreatePostalContact(certificatePrintSummary));
            result.PrintData[0].Certificates.Should().BeEquivalentTo(CreatePrintCertificates(certificatePrintSummary));
            result.PrintData[0].PostalContact.CertificateCount.Should().Be(1);
        }

        [Test]
        public void Create_Should_Return_One_Framework_PrintData_For_Single_Certificate()
        {
            var certificates = new List<CertificatePrintSummaryBase>
            {
                new FrameworkCertificatePrintSummary
                {
                    ContactName = "Bob",
                    ContactAddLine1 = "Address 2",
                    ContactPostCode = "BB1 2BB",
                    FullName = "Bob Brown",
                    FrameworkName = "Framework B",
                    PathwayName = "Pathway B",
                    FrameworkLevelName = "Level 3",
                    FrameworkCertificateNumber = "F123",
                    AchievementDate = DateTime.UtcNow
                }
            };

            var result = _sut.Create(2, certificates);

            result.PrintData.Should().HaveCount(1);

            var frameworkCertificatePrintSummary = certificates[0] as FrameworkCertificatePrintSummary;

            result.PrintData[0].Type.Should().Be("Framework");
            result.PrintData[0].PostalContact.Should().BeEquivalentTo(CreatePostalContact(frameworkCertificatePrintSummary));
            result.PrintData[0].Certificates.Should().BeEquivalentTo(CreatePrintCertificates(frameworkCertificatePrintSummary));

            result.PrintData[0].PostalContact.CertificateCount.Should().Be(1);
        }

        [Test]
        public void Create_Should_Return_Two_PrintData_Entries_For_Standard_And_Framework_With_Different_Addresses()
        {
            var certificates = new List<CertificatePrintSummaryBase>
            {
                new CertificatePrintSummary
                {
                    ContactName = "Alice",
                    ContactOrganisation = "Org A",
                    ContactAddLine1 = "Address 1",
                    ContactPostCode = "AA1 1AA",
                    LearnerGivenNames = "Alice",
                    LearnerFamilyName = "Anderson",
                    StandardName = "Standard A",
                    StandardLevel = 2,
                    OverallGrade = "Pass",
                    AchievementDate = DateTime.UtcNow
                },
                new FrameworkCertificatePrintSummary
                {
                    ContactName = "Bob",
                    ContactAddLine1 = "Address 2",
                    ContactPostCode = "BB1 2BB",
                    FullName = "Bob Brown",
                    FrameworkName = "Framework B",
                    PathwayName = "Pathway B",
                    FrameworkLevelName = "Level 3",
                    FrameworkCertificateNumber = "F123",
                    AchievementDate = DateTime.UtcNow
                }
            };

            var result = _sut.Create(3, certificates);

            result.PrintData.Should().HaveCount(2);
            result.PrintData.Should().Contain(p => p.Type == "Standard");
            result.PrintData.Should().Contain(p => p.Type == "Framework");
        }

        [Test]
        public void Create_Should_Not_Group_Standard_And_Framework_At_Same_Address()
        {
            var address = "Shared Address";

            var certificates = new List<CertificatePrintSummaryBase>
            {
                new CertificatePrintSummary
                {
                    ContactName = "Shared",
                    ContactOrganisation = "Org",
                    ContactAddLine1 = address,
                    ContactPostCode = "ZZ1 1ZZ",
                    LearnerGivenNames = "Alice",
                    LearnerFamilyName = "Smith",
                    StandardName = "Standard X",
                    StandardLevel = 2,
                    OverallGrade = "Merit",
                    AchievementDate = DateTime.UtcNow
                },
                new FrameworkCertificatePrintSummary
                {
                    ContactName = "Shared",
                    ContactAddLine1 = address,
                    ContactPostCode = "ZZ1 1ZZ",
                    FullName = "Bob Brown",
                    FrameworkName = "Framework X",
                    PathwayName = "Path",
                    FrameworkLevelName = "Level 2",
                    FrameworkCertificateNumber = "F321",
                    AchievementDate = DateTime.UtcNow
                }
            };

            var result = _sut.Create(4, certificates);

            result.PrintData.Should().HaveCount(2); // Not grouped
            result.PrintData.Count(p => p.PostalContact.AddressLine1 == address).Should().Be(2);
        }

        [Test]
        public void Create_Should_Group_Multiple_Standards_To_Same_Address()
        {
            var certificates = new List<CertificatePrintSummaryBase>
            {
                new CertificatePrintSummary
                {
                    ContactName = "Group",
                    ContactOrganisation = "Org",
                    ContactAddLine1 = "Addr",
                    ContactPostCode = "XX1 1XX",
                    LearnerGivenNames = "A1",
                    LearnerFamilyName = "S",
                    StandardName = "Std",
                    StandardLevel = 2,
                    OverallGrade = "Pass",
                    AchievementDate = DateTime.UtcNow
                },
                new CertificatePrintSummary
                {
                    ContactName = "Group",
                    ContactOrganisation = "Org",
                    ContactAddLine1 = "Addr",
                    ContactPostCode = "XX1 1XX",
                    LearnerGivenNames = "A2",
                    LearnerFamilyName = "T",
                    StandardName = "Std",
                    StandardLevel = 2,
                    OverallGrade = "Pass",
                    AchievementDate = DateTime.UtcNow
                }
            };

            var result = _sut.Create(5, certificates);

            result.PrintData.Should().HaveCount(1);
            result.PrintData[0].Type.Should().Be("Standard");
            result.PrintData[0].PostalContact.CertificateCount.Should().Be(2);
        }

        [Test]
        public void Create_Should_Group_Multiple_Frameworks_To_Same_Address()
        {
            var certificates = new List<CertificatePrintSummaryBase>
            {
                new FrameworkCertificatePrintSummary
                {
                    ContactName = "GroupF",
                    ContactAddLine1 = "FAddr",
                    ContactPostCode = "YY1 1YY",
                    FullName = "F1",
                    FrameworkName = "Fmk",
                    PathwayName = "P",
                    FrameworkLevelName = "L2",
                    FrameworkCertificateNumber = "F001",
                    AchievementDate = DateTime.UtcNow
                },
                new FrameworkCertificatePrintSummary
                {
                    ContactName = "GroupF",
                    ContactAddLine1 = "FAddr",
                    ContactPostCode = "YY1 1YY",
                    FullName = "F2",
                    FrameworkName = "Fmk",
                    PathwayName = "P",
                    FrameworkLevelName = "L2",
                    FrameworkCertificateNumber = "F002",
                    AchievementDate = DateTime.UtcNow
                }
            };

            var result = _sut.Create(6, certificates);

            result.PrintData.Should().HaveCount(1);
            result.PrintData[0].Type.Should().Be("Framework");
            result.PrintData[0].PostalContact.CertificateCount.Should().Be(2);
        }

        private static PostalContact CreatePostalContact(CertificatePrintSummary certificatePrintSummary)
        {
            return new PostalContact
            {
                Name = certificatePrintSummary.ContactName,
                EmployerName = certificatePrintSummary.ContactOrganisation,
                AddressLine1 = certificatePrintSummary.ContactAddLine1,
                Postcode = certificatePrintSummary.ContactPostCode,
                CertificateCount = 1
            };
        }

        private static PostalContact CreatePostalContact(FrameworkCertificatePrintSummary frameworkCertificatePrintSummary)
        {
            return new PostalContact
            {
                Name = frameworkCertificatePrintSummary.ContactName,
                AddressLine1 = frameworkCertificatePrintSummary.ContactAddLine1,
                Postcode = frameworkCertificatePrintSummary.ContactPostCode,
                CertificateCount = 1
            };
        }

        private List<PrintCertificate> CreatePrintCertificates(CertificatePrintSummary certificatePrintSummary)
        {
            return new List<PrintCertificate>
            {
                new StandardPrintCertificate
                {
                    ApprenticeName = $"{certificatePrintSummary.LearnerGivenNames} {certificatePrintSummary.LearnerFamilyName}",
                    CertificateNumber = certificatePrintSummary.CertificateReference,
                    LearningDetails = new StandardLearningDetails
                    {
                        StandardTitle = certificatePrintSummary.StandardName,
                        Level = certificatePrintSummary.StandardLevel.ToString(),
                        CoronationEmblem = certificatePrintSummary.CoronationEmblem,
                        Option = certificatePrintSummary.CourseOption,
                        Grade = certificatePrintSummary.OverallGrade,
                        AchievementDate = $"{certificatePrintSummary.AchievementDate.Value:dd MMMM yyyy}",
                        GradeText = "Achieved grade ",

                    }
                }
            };
        }

        private List<PrintCertificate> CreatePrintCertificates(FrameworkCertificatePrintSummary frameworkCertificatePrintSummary)
        {
            return new List<PrintCertificate>
            {
                new FrameworkPrintCertificate
                {
                    ApprenticeName = frameworkCertificatePrintSummary.FullName,
                    CertificateNumber = frameworkCertificatePrintSummary.CertificateReference,
                    LearningDetails = new FrameworkLearningDetails
                    {
                        FrameworkName = frameworkCertificatePrintSummary.FrameworkName,
                        PathwayName = frameworkCertificatePrintSummary.PathwayName,
                        LevelName = frameworkCertificatePrintSummary.FrameworkLevelName,
                        AchievementDate = $"{frameworkCertificatePrintSummary.AchievementDate.Value:dd MMMM yyyy}",
                        FrameworkCertificateNumber = frameworkCertificatePrintSummary.FrameworkCertificateNumber
                    }
                }
            };
        }
    }
}
