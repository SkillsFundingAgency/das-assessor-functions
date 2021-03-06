﻿using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface ICertificateService
    {
        Task<ValidationResponse> ProcessCertificatesPrintStatusUpdate(CertificatePrintStatusUpdate certificatePrintStatusUpdate);
    }
}
