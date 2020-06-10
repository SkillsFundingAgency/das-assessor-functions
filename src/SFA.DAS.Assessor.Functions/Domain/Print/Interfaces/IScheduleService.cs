using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IScheduleService
    {
        Task<Schedule> Get();
        Task Save(Schedule schedule);
    }
}
