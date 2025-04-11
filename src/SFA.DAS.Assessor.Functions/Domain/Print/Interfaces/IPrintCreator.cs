using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IPrintCreator
    {
        PrintOutput Create(int batchNumber, IEnumerable<CertificatePrintSummaryBase> certificates);
    }
}
