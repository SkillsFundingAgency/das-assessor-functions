using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface INotificationService
    {
        Task SendPrintRequest(int batchNumber, List<Certificate> certificates, string certificatesFileName);
        Task SendSasToken(string message);
    }
}
