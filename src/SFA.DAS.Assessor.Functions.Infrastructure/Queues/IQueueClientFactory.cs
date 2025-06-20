using Azure.Storage.Queues;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Queues
{
    public interface IQueueClientFactory
    {
        /// <summary>
        /// Retrieves a cached QueueClient for the specified queue name. Creates and caches it if not already present.
        /// </summary>
        /// <param name="queueName">The name of the Azure Storage Queue.</param>
        /// <returns>An instance of QueueClient.</returns>
        QueueClient GetQueueClient(string queueName);
    }
}
