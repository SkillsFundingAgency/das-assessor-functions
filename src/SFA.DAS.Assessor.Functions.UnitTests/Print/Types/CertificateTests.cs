using System;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Types
{
    public class CertificateTests
    {
        [Test]
        public void FromCertificatePrintSummary_ReturnsNewCertificate_WithPropertiesOfSummary()
        {
            CertificatePrintSummary certificateSummary = GetCertificatePrintSummary();

            Assert.Multiple(() =>
            {
                Assert.That(certificateSummary.Uln, Is.EqualTo(certificateSummary.Uln));
                Assert.That(certificateSummary.StandardCode, Is.EqualTo(certificateSummary.StandardCode));
                Assert.That(certificateSummary.ProviderUkPrn, Is.EqualTo(certificateSummary.ProviderUkPrn));
                Assert.That(certificateSummary.EndPointAssessorOrganisationId, Is.EqualTo(certificateSummary.EndPointAssessorOrganisationId));
                Assert.That(certificateSummary.EndPointAssessorOrganisationName, Is.EqualTo(certificateSummary.EndPointAssessorOrganisationName));
                Assert.That(certificateSummary.CertificateReference, Is.EqualTo(certificateSummary.CertificateReference));
                Assert.That(certificateSummary.LearnerGivenNames, Is.EqualTo(certificateSummary.LearnerGivenNames));
                Assert.That(certificateSummary.LearnerFamilyName, Is.EqualTo(certificateSummary.LearnerFamilyName));
                Assert.That(certificateSummary.StandardName, Is.EqualTo(certificateSummary.StandardName));
                Assert.That(certificateSummary.StandardLevel, Is.EqualTo(certificateSummary.StandardLevel));
                Assert.That(certificateSummary.CoronationEmblem, Is.EqualTo(certificateSummary.CoronationEmblem));
                Assert.That(certificateSummary.ContactName, Is.EqualTo(certificateSummary.ContactName));
                Assert.That(certificateSummary.ContactOrganisation, Is.EqualTo(certificateSummary.ContactOrganisation));
                Assert.That(certificateSummary.ContactAddLine1, Is.EqualTo(certificateSummary.ContactAddLine1));
                Assert.That(certificateSummary.ContactAddLine2, Is.EqualTo(certificateSummary.ContactAddLine2));
                Assert.That(certificateSummary.ContactAddLine3, Is.EqualTo(certificateSummary.ContactAddLine3));
                Assert.That(certificateSummary.ContactAddLine4, Is.EqualTo(certificateSummary.ContactAddLine4));
                Assert.That(certificateSummary.ContactPostCode, Is.EqualTo(certificateSummary.ContactPostCode));
                Assert.That(certificateSummary.AchievementDate, Is.EqualTo(certificateSummary.AchievementDate));
                Assert.That(certificateSummary.CourseOption, Is.EqualTo(certificateSummary.CourseOption));
                Assert.That(certificateSummary.OverallGrade, Is.EqualTo(certificateSummary.OverallGrade));
                Assert.That(certificateSummary.Department, Is.EqualTo(certificateSummary.Department));
                Assert.That(certificateSummary.FullName, Is.EqualTo(certificateSummary.FullName));
                Assert.That(certificateSummary.Status, Is.EqualTo(certificateSummary.Status));
            });
        }

        private static CertificatePrintSummary GetCertificatePrintSummary()
        {
            var rng = new Random();
            return new CertificatePrintSummary()
            {
                Uln = rng.NextInt64(),
                StandardCode = rng.Next(),
                ProviderUkPrn = rng.Next(),
                EndPointAssessorOrganisationId = $"{Guid.NewGuid()}",
                EndPointAssessorOrganisationName = $"{Guid.NewGuid()}",
                CertificateReference = $"{Guid.NewGuid()}",
                BatchNumber = $"{Guid.NewGuid()}",
                LearnerGivenNames = $"{Guid.NewGuid()}",
                LearnerFamilyName = $"{Guid.NewGuid()}",
                StandardName = $"{Guid.NewGuid()}",
                StandardLevel = rng.Next(),
                CoronationEmblem = Convert.ToBoolean(rng.Next(0, 1)),
                ContactName = $"{Guid.NewGuid()}",
                ContactOrganisation = $"{Guid.NewGuid()}",
                ContactAddLine1 = $"{Guid.NewGuid()}",
                ContactAddLine2 = $"{Guid.NewGuid()}",
                ContactAddLine3 = $"{Guid.NewGuid()}",
                ContactAddLine4 = $"{Guid.NewGuid()}",
                ContactPostCode = $"{Guid.NewGuid()}",
                AchievementDate = DateTime.UtcNow,
                CourseOption = $"{Guid.NewGuid()}",
                OverallGrade = $"{Guid.NewGuid()}",
                Department = $"{Guid.NewGuid()}",
                FullName = $"{Guid.NewGuid()}",
                Status = $"{Guid.NewGuid()}"
            };
        }
    }
}
