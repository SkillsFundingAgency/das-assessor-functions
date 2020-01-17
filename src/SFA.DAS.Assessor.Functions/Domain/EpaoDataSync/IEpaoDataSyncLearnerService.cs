using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public interface IEpaoDataSyncLearnerService
    {
        Task<EpaoDataSyncProviderMessage> ProcessLearners(EpaoDataSyncProviderMessage providerMessage);
    }
}
