using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain
{
    public interface IEpaoDataSyncProviderService
    {
        Task<List<EpaoDataSyncProviderMessage>> ProcessProviders();
        Task<DateTime> GetLastRunDateTime();
        Task SetLastRunDateTime(DateTime nextRunDateTime);
    }
}
