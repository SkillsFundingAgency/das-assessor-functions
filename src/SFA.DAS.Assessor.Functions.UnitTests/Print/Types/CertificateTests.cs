using System;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.UnitTests.Print.Types
{
    public class CertificateTests
    {
        [Test]
        public void FromCertificatePrintSummary_ReturnsNewCertificate_WithPropertiesOfSummary()
        {
            CertificatePrintSummary certificateSummary = GetCertificatePrintSummary();
            Certificate certificate = Certificate.FromCertificatePrintSummary(certificateSummary);

            Assert.Multiple(() =>
            {
                Assert.That(certificate.Uln, Is.EqualTo(certificateSummary.Uln));
                Assert.That(certificate.StandardCode, Is.EqualTo(certificateSummary.StandardCode));
                Assert.That(certificate.ProviderUkPrn, Is.EqualTo(certificateSummary.ProviderUkPrn));
                Assert.That(certificate.EndPointAssessorOrganisationId, Is.EqualTo(certificateSummary.EndPointAssessorOrganisationId));
                Assert.That(certificate.EndPointAssessorOrganisationName, Is.EqualTo(certificateSummary.EndPointAssessorOrganisationName));
                Assert.That(certificate.CertificateReference, Is.EqualTo(certificateSummary.CertificateReference));
                Assert.That(certificate.LearnerGivenNames, Is.EqualTo(certificateSummary.LearnerGivenNames));
                Assert.That(certificate.LearnerFamilyName, Is.EqualTo(certificateSummary.LearnerFamilyName));
                Assert.That(certificate.StandardName, Is.EqualTo(certificateSummary.StandardName));
                Assert.That(certificate.StandardLevel, Is.EqualTo(certificateSummary.StandardLevel));
                Assert.That(certificate.CoronationEmblem, Is.EqualTo(certificateSummary.CoronationEmblem));
                Assert.That(certificate.ContactName, Is.EqualTo(certificateSummary.ContactName));
                Assert.That(certificate.ContactOrganisation, Is.EqualTo(certificateSummary.ContactOrganisation));
                Assert.That(certificate.ContactAddLine1, Is.EqualTo(certificateSummary.ContactAddLine1));
                Assert.That(certificate.ContactAddLine2, Is.EqualTo(certificateSummary.ContactAddLine2));
                Assert.That(certificate.ContactAddLine3, Is.EqualTo(certificateSummary.ContactAddLine3));
                Assert.That(certificate.ContactAddLine4, Is.EqualTo(certificateSummary.ContactAddLine4));
                Assert.That(certificate.ContactPostCode, Is.EqualTo(certificateSummary.ContactPostCode));
                Assert.That(certificate.AchievementDate, Is.EqualTo(certificateSummary.AchievementDate));
                Assert.That(certificate.CourseOption, Is.EqualTo(certificateSummary.CourseOption));
                Assert.That(certificate.OverallGrade, Is.EqualTo(certificateSummary.OverallGrade));
                Assert.That(certificate.Department, Is.EqualTo(certificateSummary.Department));
                Assert.That(certificate.FullName, Is.EqualTo(certificateSummary.FullName));
                Assert.That(certificate.Status, Is.EqualTo(certificateSummary.Status));
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
