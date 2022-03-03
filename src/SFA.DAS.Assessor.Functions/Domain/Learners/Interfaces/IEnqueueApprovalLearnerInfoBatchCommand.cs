using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IEnqueueApprovalLearnerInfoBatchCommand
    {
        IAsyncCollector<ProcessApprovalBatchLearnersCommand> StorageQueue { get; set; }

        Task Execute();
    }
}