using SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Interfaces
{
    public interface IEpaoDataSyncProviderService
    {
        Task<List<EpaoDataSyncProviderMessage>> ProcessProviders();
        Task<DateTime> GetLastRunDateTime();
        Task SetLastRunDateTime(DateTime nextRunDateTime);
    }
}
