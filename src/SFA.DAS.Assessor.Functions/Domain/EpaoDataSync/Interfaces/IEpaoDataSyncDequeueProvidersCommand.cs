using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces
{
    public interface IEpaoDataSyncDequeueProvidersCommand
    {
        IStorageQueue StorageQueue { get; set; }

        Task Execute(string message);
    }
}
