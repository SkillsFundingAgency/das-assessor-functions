using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IImportLearnersCommand
    {
        Task Execute();
    }
}
