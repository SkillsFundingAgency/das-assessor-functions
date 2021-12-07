using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsAccessorSettingService
    {
        Task<DateTime> GetLastRunDateTime();
        Task SetLastRunDateTime(DateTime lastRunDateTime);
    }
}
