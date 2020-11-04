using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IBatchService
    {
        Task<Batch> Get(int batchNumber);
        Task<int?> GetOrCreatePrintBatchReadyToPrint(DateTime scheduledDate);
        Task<List<Certificate>> GetCertificatesForBatchNumber(int batchNumber);
        Task Update(Batch batch, ICollector<string> StorageQueue);
    }
}
