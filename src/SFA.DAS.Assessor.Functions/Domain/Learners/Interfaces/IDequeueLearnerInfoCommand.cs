using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IDequeueLearnerInfoCommand
    {
        Task Execute(string message);
    }
}
