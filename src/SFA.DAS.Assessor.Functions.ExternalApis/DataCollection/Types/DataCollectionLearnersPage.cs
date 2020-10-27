using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types
{
    public class DataCollectionLearnersPage
    {
        public DataCollectionLearnersPage()
        {
            Learners = new List<DataCollectionLearner>();
            PagingInfo = new DataCollectionPagingInfo();
        }

        public List<DataCollectionLearner> Learners { get; set; }
        public DataCollectionPagingInfo PagingInfo { get; set; }
    }
}
