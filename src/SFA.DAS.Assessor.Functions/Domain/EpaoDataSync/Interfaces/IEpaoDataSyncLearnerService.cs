using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces
{
    public interface IEpaoDataSyncLearnerService
    {
        Task<EpaoDataSyncProviderMessage> ProcessLearners(EpaoDataSyncProviderMessage providerMessage);
    }
}
