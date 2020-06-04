using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public enum CertificateStatus
    {
        ToBePrinted
    }

    public interface ICertificateClient
    {
        Task<IEnumerable<Certificate>> Get(CertificateStatus status);
        Task Save(IEnumerable<Certificate> certificates);
    }
}
