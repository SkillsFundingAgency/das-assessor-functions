using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IEnqueueLearnerInfoCommand
    {
        IAsyncCollector<UpdateLearnersInfoMessage> StorageQueue { get; set; }

        Task Execute(string message);
    }
}