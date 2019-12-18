using System;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection
{
    public class DataCollectionLearner
    {
        public int? Ukprn { get; set; }
        public string LearnRefNumber { get; set; }
        public long? Uln { get; set; }
        public string FamilyName { get; set; }
        public string GivenNames { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NiNumber { get; set; }
        public List<DataCollectionLearningDelivery> LearningDeliveries { get; set; }
    }
}
