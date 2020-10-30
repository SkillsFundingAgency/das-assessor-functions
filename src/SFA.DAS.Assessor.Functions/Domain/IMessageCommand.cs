using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IMessageCommand
    {
        Task Execute(string message);
    }
}
