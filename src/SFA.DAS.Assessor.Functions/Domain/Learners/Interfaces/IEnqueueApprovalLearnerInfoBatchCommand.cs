using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IEnqueueApprovalLearnerInfoBatchCommand
    {
        Task Execute();
    }
}