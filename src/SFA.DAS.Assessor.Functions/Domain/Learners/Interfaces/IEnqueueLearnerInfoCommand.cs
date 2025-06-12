using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IEnqueueLearnerInfoCommand
    {
        IAsyncCollector<UpdateLearnersInfoMessage> StorageQueue { get; set; }

        Task Execute(string message);
    }
}