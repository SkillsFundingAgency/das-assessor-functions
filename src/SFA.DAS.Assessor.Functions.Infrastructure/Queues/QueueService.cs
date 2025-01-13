using Azure.Storage.Queues;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Queues
{
    public class QueueService : IQueueService
    {
        private readonly IQueueClientFactory _queueClientFactory;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IQueueClientFactory queueClientFactory, ILogger<QueueService> logger)
        {
            _queueClientFactory = queueClientFactory;
            _logger = logger;
        }

        public async Task EnqueueMessageAsync<T>(string queueName, T message) where T : class
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be provided.", nameof(queueName));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                string messageJson = JsonConvert.SerializeObject(message);

                QueueClient queueClient = _queueClientFactory.GetQueueClient(queueName);

                await queueClient.SendMessageAsync(messageJson);

                _logger.LogInformation($"Message successfully enqueued to queue '{queueName}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to enqueue message to queue '{queueName}'.");
                throw; // Re-throw to allow higher-level handling if necessary
            }
        }
    }
}
