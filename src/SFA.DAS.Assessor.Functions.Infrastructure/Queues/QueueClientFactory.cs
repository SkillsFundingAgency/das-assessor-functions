using Azure.Storage.Queues;
using System;
using System.Collections.Concurrent;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Queues
{
    public class QueueClientFactory : IQueueClientFactory
    {
        private readonly string _connectionString;
        private readonly ConcurrentDictionary<string, QueueClient> _queueClients;

        public QueueClientFactory(string connectionString)
        {
            _connectionString = connectionString;
            _queueClients = new ConcurrentDictionary<string, QueueClient>(StringComparer.OrdinalIgnoreCase);
        }

        public QueueClient GetQueueClient(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be provided.", nameof(queueName));

            return _queueClients.GetOrAdd(queueName, name =>
            {
                var queueClient = new QueueClient(_connectionString, name);
                queueClient.CreateIfNotExists(); 
                return queueClient;
            });
        }
    }
}
