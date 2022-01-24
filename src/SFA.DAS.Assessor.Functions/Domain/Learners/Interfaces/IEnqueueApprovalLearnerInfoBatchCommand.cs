using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IEnqueueApprovalLearnerInfoBatchCommand
    {
        ICollector<string> StorageQueue { get; set; }

        Task Execute();
    }
}