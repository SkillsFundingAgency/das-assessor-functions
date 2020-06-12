using Microsoft.WindowsAzure.Storage.Queue;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public class StorageQueue : IStorageQueue
    {
        private readonly CloudQueue _cloudQueue;

        public StorageQueue(CloudQueue cloudQueue)
        {
            _cloudQueue = cloudQueue;
        }

        public async Task AddMessageAsync(CloudQueueMessage message)
        {
            await _cloudQueue.AddMessageAsync(message);
        }
    }
}
