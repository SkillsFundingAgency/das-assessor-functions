using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Queues
{
    public interface IQueueService
    {
        /// <summary>
        /// Enqueues a message to the specified Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T">The type of the message to enqueue.</typeparam>
        /// <param name="queueName">The name of the Azure Storage Queue.</param>
        /// <param name="message">The message object to enqueue.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task EnqueueMessageAsync<T>(string queueName, T message) where T : class;
    }
}
