using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;

        public CertificateService(IAssessorServiceApiClient assessorServiceApiClient)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
        }

        public async Task<IEnumerable<Certificate>> Get(Interfaces.CertificateStatus status)
        {
            switch (status)
            {
                case Interfaces.CertificateStatus.ToBePrinted:
                    return await ToBePrinted();

                default:
                    return new List<Certificate>();
            }
        }

        public async Task<ValidationResponse> Save(IEnumerable<Certificate> certificates)
        {
            return await _assessorServiceApiClient.UpdatePrintStatus(certificates.Select(c => new CertificatePrintStatus 
            { 
                 BatchNumber = c.BatchId.Value,
                 CertificateReference = c.CertificateReference,
                 Status = c.Status,
                 StatusChangedAt = c.StatusDate.Value,
                 ReasonForChange = c.Reason
            }));
        }

        private async Task<IEnumerable<Certificate>> ToBePrinted()
        {
            var response = await _assessorServiceApiClient.GetCertificatesToBePrinted();
            return response.Certificates.Select(Map);
        }

        private Certificate Map(CertificateToBePrintedSummary certificateToBePrinted)
        {
            var certificate = new Certificate
            {
                Uln = certificateToBePrinted.Uln,
                StandardCode = certificateToBePrinted.StandardCode,
                ProviderUkPrn = certificateToBePrinted.ProviderUkPrn,
                EndPointAssessorOrganisationId = certificateToBePrinted.EndPointAssessorOrganisationId,
                EndPointAssessorOrganisationName = certificateToBePrinted.EndPointAssessorOrganisationName,
                CertificateReference = certificateToBePrinted.CertificateReference,
                LearnerGivenNames = certificateToBePrinted.LearnerGivenNames,
                LearnerFamilyName = certificateToBePrinted.LearnerFamilyName,
                StandardName = certificateToBePrinted.StandardName,
                StandardLevel = certificateToBePrinted.StandardLevel,
                ContactName = certificateToBePrinted.ContactName,
                ContactOrganisation = certificateToBePrinted.ContactOrganisation,
                ContactAddLine1 = certificateToBePrinted.ContactAddLine1,
                ContactAddLine2 = certificateToBePrinted.ContactAddLine2,
                ContactAddLine3 = certificateToBePrinted.ContactAddLine3,
                ContactAddLine4 = certificateToBePrinted.ContactAddLine4,
                ContactPostCode = certificateToBePrinted.ContactPostCode,
                AchievementDate = certificateToBePrinted.AchievementDate,
                CourseOption = certificateToBePrinted.CourseOption,
                OverallGrade = certificateToBePrinted.OverallGrade,
                Department = certificateToBePrinted.Department,
                FullName = certificateToBePrinted.FullName,
                Status = certificateToBePrinted.Status
            };

            return certificate;
        }
    }
}
