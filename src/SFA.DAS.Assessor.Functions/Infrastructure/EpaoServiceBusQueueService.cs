using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class EpaoServiceBusQueueService : IEpaoServiceBusQueueService
    {
        private readonly IQueueClient _serviceBusQueue; 
        
        public EpaoServiceBusQueueService(string connectionString, string queueName)
        {
            _serviceBusQueue = GetQueue(connectionString, queueName);
        }

        public async Task SerializeAndQueueMessage(EpaoDataSyncProviderMessage epaoDataSyncProviderMessage)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(epaoDataSyncProviderMessage)));
            await _serviceBusQueue.SendAsync(message);
        }

        public EpaoDataSyncProviderMessage DeserializeMessage(Message message)
        {
            return JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(Encoding.UTF8.GetString(message.Body));
        }

        private IQueueClient GetQueue(string connectionString, string queueName)
        {
            return new QueueClient(connectionString, queueName);
        }
    }
}
