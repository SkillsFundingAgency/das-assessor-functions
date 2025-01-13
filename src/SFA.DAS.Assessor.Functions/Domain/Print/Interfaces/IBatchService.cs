using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IBatchService
    {
        Task<Batch> Get(int batchNumber);
        Task<Batch> BuildPrintBatchReadyToPrint(DateTime scheduledDate, int maxCertificatesToBeAdded);
        Task<List<Certificate>> GetCertificatesForBatchNumber(int batchNumber);
        Task<List<CertificatePrintStatusUpdateMessage>> Update(Batch batch);
    }
}
