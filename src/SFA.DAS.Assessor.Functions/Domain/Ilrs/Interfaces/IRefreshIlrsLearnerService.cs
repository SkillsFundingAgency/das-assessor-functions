using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsLearnerService
    {
        Task<RefreshIlrsProviderMessage> ProcessLearners(RefreshIlrsProviderMessage providerMessage);
    }
}
