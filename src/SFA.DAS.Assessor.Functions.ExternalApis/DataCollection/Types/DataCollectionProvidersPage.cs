using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types
{
    public class DataCollectionProvidersPage
    {
        public DataCollectionProvidersPage()
        {
            Providers = new List<int>();
            PagingInfo = new DataCollectionPagingInfo();
        }

        public List<int> Providers { get; set; }
        public DataCollectionPagingInfo PagingInfo { get; set; }
    }
}
