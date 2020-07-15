using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface ICommand
    {
        Task Execute();
    }
}
