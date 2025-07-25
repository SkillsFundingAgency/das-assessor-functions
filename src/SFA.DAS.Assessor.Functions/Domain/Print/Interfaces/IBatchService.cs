using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IBatchService
    {
        Task<Batch> Get(int batchNumber);
        Task<Batch> BuildPrintBatchReadyToPrint(DateTime scheduledDate, int maxCertificatesToBeAdded);
        Task<List<CertificatePrintSummaryBase>> GetCertificatesForBatchNumber(int batchNumber);
        Task<List<CertificatePrintStatusUpdateMessage>> Update(Batch batch);
    }
}
