using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface INotificationService
    {
        Task SendPrintRequest(int batchNumber, int certificatesCount, string certificatesFileName);
        Task SendSasToken(string message);
    }
}
