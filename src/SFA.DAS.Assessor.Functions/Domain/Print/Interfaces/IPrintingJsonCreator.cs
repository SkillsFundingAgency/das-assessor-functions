using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IPrintingJsonCreator
    {
        void Create(int batchNumber, List<CertificateResponse> certificates, string certificatesFileName);
    }
}
