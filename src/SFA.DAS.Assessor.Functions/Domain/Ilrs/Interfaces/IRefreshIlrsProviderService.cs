using SFA.DAS.Assessor.Functions.Domain.Ilrs.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsProviderService
    {
        Task<List<RefreshIlrsProviderMessage>> ProcessProviders();
        Task<DateTime> GetLastRunDateTime();
        Task SetLastRunDateTime(DateTime nextRunDateTime);
    }
}
