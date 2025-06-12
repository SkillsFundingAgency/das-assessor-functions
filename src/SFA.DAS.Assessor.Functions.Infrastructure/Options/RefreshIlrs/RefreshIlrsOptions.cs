using System;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs
{
    public class RefreshIlrsOptions
    {
        public int EnqueueProvidersMaxPastDueMinutes { get; set; }
        public int ProviderPageSize { get; set; }
        public DateTime ProviderInitialRunDate { get; set; }
        public int LearnerPageSize { get; set; }
        public string LearnerFundModels { get; set; }
        public string AcademicYearsOverride { get; set; }
    }
}
