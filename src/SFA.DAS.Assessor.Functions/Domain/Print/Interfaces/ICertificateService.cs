using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public enum CertificateStatus
    {
        ToBePrinted
    }

    public interface ICertificateService
    {
        Task<IEnumerable<Certificate>> Get(CertificateStatus status);
        Task<ValidationResponse> Save(IEnumerable<Certificate> certificates);
    }
}
