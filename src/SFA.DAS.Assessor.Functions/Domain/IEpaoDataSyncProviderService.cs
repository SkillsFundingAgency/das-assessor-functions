using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public interface IEpaoDataSyncProviderService
    {
        Task ProcessProviders();
    }
}
