using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IPrintingSpreadsheetCreator
    {
        void Create(int batchNumber, IEnumerable<CertificateToBePrintedSummary> certificates);
    }
}
