using System;

namespace SFA.DAS.Assessor.Functions
{
    public class EpaoDataSync
    {
        public const string ProviderQueueName = "epao-data-sync-providers";

        public int ProviderPageSize { get; set; }
        public DateTime ProviderInitialRunDate { get; set; }
        public int LearnerPageSize { get; set; }
        public string LearnerFundModels { get; set; }
    }
}
