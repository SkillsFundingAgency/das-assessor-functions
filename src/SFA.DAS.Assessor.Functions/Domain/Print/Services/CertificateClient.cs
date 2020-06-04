using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{


    public class CertificateClient : ICertificateClient
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;

        public CertificateClient(IAssessorServiceApiClient assessorServiceApiClient)
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

        public async Task Save(IEnumerable<Certificate> certificates)
        {
            await _assessorServiceApiClient.UpdatePrintStatus(certificates.Select(c => new CertificatePrintStatus 
            { 
                 BatchNumber = c.BatchId.Value,
                 CertificateReference = c.CertificateReference,
                 Status = c.Status,
                 StatusChangedAt = c.StatusDate.Value
            }));
        }

        private async Task<IEnumerable<Certificate>> ToBePrinted()
        {
            var certificates = new List<Certificate>();

            var certificateResponses = await _assessorServiceApiClient.GetCertificatesToBePrinted();

            return certificateResponses.Select(c => Map(c));
        }

        private Certificate Map(CertificateResponse response)
        {
            var newCertificate = new Certificate
            {
                Uln = response.Uln,
                StandardCode = response.StandardCode,
                ProviderUkPrn = response.ProviderUkPrn,
                EndPointAssessorOrganisationId = response.EndPointAssessorOrganisationId,
                EndPointAssessorOrganisationName = response.EndPointAssessorOrganisationName,
                CertificateReference = response.CertificateReference,
                Status = response.Status
            };

            if (response.CertificateData != null)
            {
                newCertificate.LearnerGivenNames = response.CertificateData.LearnerGivenNames;
                newCertificate.LearnerFamilyName = response.CertificateData.LearnerFamilyName;
                newCertificate.StandardReference = response.CertificateData.StandardReference;
                newCertificate.StandardName = response.CertificateData.StandardName;
                newCertificate.StandardLevel = response.CertificateData.StandardLevel;
                newCertificate.StandardPublicationDate = response.CertificateData.StandardPublicationDate;
                newCertificate.ContactName = response.CertificateData.ContactName;
                newCertificate.ContactOrganisation = response.CertificateData.ContactOrganisation;
                newCertificate.ContactAddLine1 = response.CertificateData.ContactAddLine1;
                newCertificate.ContactAddLine2 = response.CertificateData.ContactAddLine2;
                newCertificate.ContactAddLine3 = response.CertificateData.ContactAddLine3;
                newCertificate.ContactAddLine4 = response.CertificateData.ContactAddLine4;
                newCertificate.ContactPostCode = response.CertificateData.ContactPostCode;
                newCertificate.Registration = response.CertificateData.Registration;
                newCertificate.ProviderName = response.CertificateData.ProviderName;
                newCertificate.LearningStartDate = response.CertificateData.LearningStartDate;
                newCertificate.AchievementDate = response.CertificateData.AchievementDate;
                newCertificate.CourseOption = response.CertificateData.CourseOption;
                newCertificate.OverallGrade = response.CertificateData.OverallGrade;
                newCertificate.Department = response.CertificateData.Department;
                newCertificate.FullName = response.CertificateData.FullName;
            }

            return newCertificate;
        }
    }
}
