using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Assessor.Functions
{
    public class EpaoDataSync
    {
        public int ProviderPageSize { get; set; }
        public DateTime ProviderInitialRunDate { get; set; }
        public int LearnerPageSize { get; set; }
        public string LearnerFundModels { get; set; }
    }
}
