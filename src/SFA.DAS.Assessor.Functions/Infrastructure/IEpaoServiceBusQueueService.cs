using Microsoft.Azure.ServiceBus;
using SFA.DAS.Assessor.Functions.Domain;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public interface IEpaoServiceBusQueueService
    {
        Task SerializeAndQueueMessage(EpaoDataSyncProviderMessage message);
        EpaoDataSyncProviderMessage DeserializeMessage(Message message);
    }
}
