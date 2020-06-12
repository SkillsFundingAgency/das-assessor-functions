using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces
{
    public interface IEpaoDataSyncEnqueueProvidersCommand : ICommand
    {
        IStorageQueue StorageQueue { get; set; }
    }
}
