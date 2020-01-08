using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Assessor.Functions
{
    public class EpaoDataSync
    {
        public const string ProviderQueueName = "epao-data-sync-providers";

        public int ProviderPageSize { get; set; }
        public DateTime ProviderInitialRunDate { get; set; }
        public int LearnerPageSize { get; set; }
        public string LearnerFundModels { get; set; }

        /// <summary>
        /// Gets a list of learner fund models
        /// </summary>
        /// <remarks>
        /// This parses the configuration string as the Options pattern does not correctly deserialize a List{int}
        /// </remarks>
        public List<int> LearnerFundModelList => LearnerFundModels.Split(',').ToList().ConvertAll(p => int.Parse(p));
    }
}
