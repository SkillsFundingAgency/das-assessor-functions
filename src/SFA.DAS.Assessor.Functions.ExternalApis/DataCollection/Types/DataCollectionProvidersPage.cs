using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Types
{
    public class DataCollectionProvidersPage
    {
        public List<int> Providers { get; set; }
        public DataCollectionPagingInfo PagingInfo { get; set; }
    }
}
