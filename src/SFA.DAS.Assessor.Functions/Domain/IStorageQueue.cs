
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IStorageQueue
    {
        Task AddMessageAsync(CloudQueueMessage message);
    }
}
