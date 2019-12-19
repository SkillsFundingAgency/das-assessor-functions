using Microsoft.Azure.Storage.Queue;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public interface IStorageQueueService
    {
        Task AddMessageAsync(CloudQueueMessage message);
    }
}
