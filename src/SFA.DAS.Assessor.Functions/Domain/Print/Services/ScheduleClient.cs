using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class ScheduleClient : IScheduleClient
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;

        public ScheduleClient(IAssessorServiceApiClient assessorServiceApiClient)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
        }

        public async Task<Schedule> Get()
        {
            var scheduleRun = await _assessorServiceApiClient.GetSchedule(ScheduleType.PrintRun);

            if (scheduleRun == null) return null;

            return new Schedule
            {
                Id = scheduleRun.Id,
                RunTime = scheduleRun.RunTime
            };
        }

        public Task Save(Schedule schedule)
        {
            return _assessorServiceApiClient.CompleteSchedule(schedule.Id);
        }
    }
}
