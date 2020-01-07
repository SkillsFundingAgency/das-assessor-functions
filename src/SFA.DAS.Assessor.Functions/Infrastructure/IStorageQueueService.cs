using SFA.DAS.Assessor.Functions.Domain;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public interface IStorageQueueService
    {
        Task SerializeAndQueueMessage(EpaoDataSyncProviderMessage message);
        EpaoDataSyncProviderMessage DeserializeMessage(string message);
    }
}
