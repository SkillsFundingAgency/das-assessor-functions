using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IDequeueLearnerInfoCommand
    {
        Task Execute(string message);
    }
}
