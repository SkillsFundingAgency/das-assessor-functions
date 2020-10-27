using System;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class RefreshIlrsSettings
    {
        public string EnqueueProvidersSchedule { get; set; }
        public int EnqueueProvidersMaxPastDueMinutes { get; set; }
        public int ProviderPageSize { get; set; }
        public DateTime ProviderInitialRunDate { get; set; }
        public int LearnerPageSize { get; set; }
        public string LearnerFundModels { get; set; }
    }
}
