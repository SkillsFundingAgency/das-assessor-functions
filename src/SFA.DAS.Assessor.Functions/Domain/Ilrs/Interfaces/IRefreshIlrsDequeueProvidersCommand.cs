using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsDequeueProvidersCommand
    {
        IStorageQueue StorageQueue { get; set; }

        Task Execute(string message);
    }
}
