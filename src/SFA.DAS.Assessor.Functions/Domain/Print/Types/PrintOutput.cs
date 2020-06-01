using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class PrintOutput
    {
        public BatchData Batch { get; set; }
        public List<PrintData> PrintData { get; set; }
    }
}
