using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IQueueCommand
    {
        Task Execute(ICollector<string> messageQueue);
    }
}
