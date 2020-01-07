using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class StorageQueueService : IStorageQueueService
    {
        private readonly IConfiguration _configuration;
        private readonly CloudQueue _cloudQueue; 
        
        public StorageQueueService(IConfiguration configuration, string queueName)
        {
            _configuration = configuration;
            _cloudQueue = GetQueue(queueName);
        }

        public async Task SerializeAndQueueMessage(EpaoDataSyncProviderMessage message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            await _cloudQueue.AddMessageAsync(new CloudQueueMessage(jsonMessage));
        }

        public EpaoDataSyncProviderMessage DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<EpaoDataSyncProviderMessage>(message);
        }

        public async Task AddMessageAsync(CloudQueueMessage message)
        {
            await _cloudQueue.AddMessageAsync(message);
        }

        private CloudQueue GetQueue(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration["ConfigurationStorageConnectionString"]);
            var queueClient = storageAccount.CreateCloudQueueClient();
            return queueClient.GetQueueReference(queueName);
        }
    }
}
