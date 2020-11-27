using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(IAssessorServiceApiClient assessorServiceApiClient, ILogger<CertificateService> logger)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public async Task ProcessCertificatesPrintStatusUpdate(CertificatePrintStatusUpdate certificatePrintStatusUpdate)
        {
            var model = new CertificatesPrintStatusUpdateRequest()
            {
                BatchNumber = certificatePrintStatusUpdate.BatchNumber,
                CertificateReference = certificatePrintStatusUpdate.CertificateReference,
                ReasonForChange = certificatePrintStatusUpdate.ReasonForChange,
                Status = certificatePrintStatusUpdate.Status,
                StatusAt = certificatePrintStatusUpdate.StatusAt
            };

            await _assessorServiceApiClient.UpdateCertificatesPrintStatus(model);
        }
    }
}
