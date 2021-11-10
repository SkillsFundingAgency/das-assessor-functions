using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces
{
    public interface IRefreshProvidersCommand
    {
        Task Execute();
    }
}
