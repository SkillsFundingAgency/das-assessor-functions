using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsDequeueProvidersCommand
    {
        Task Execute(string message);
    }
}
