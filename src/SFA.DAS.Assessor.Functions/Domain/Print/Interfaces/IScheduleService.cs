using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IScheduleService
    {
        Task<Schedule> Get();
        Task Save(Schedule schedule);
        Task UpdateStatus(Schedule schedule, ScheduleRunStatus status);
    }
}
