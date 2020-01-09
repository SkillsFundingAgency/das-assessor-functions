using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public interface IEpaoDataSyncLearnerService
    {
        Task ProcessLearners(EpaoDataSyncProviderMessage providerMessage);
    }
}
