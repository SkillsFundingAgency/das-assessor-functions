using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IDequeueLearnerInfoCommand
    {
        Task Execute(string message);
    }
}
