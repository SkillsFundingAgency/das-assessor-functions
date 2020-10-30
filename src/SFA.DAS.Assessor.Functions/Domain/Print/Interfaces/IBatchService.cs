using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IBatchService
    {
        Task<Batch> Get(int batchNumber);
        Task<int?> CreateNextBatchToBePrinted(DateTime scheduledDate);
        Task<int?> GetNextBatchNumberToBePrinted();
        Task<List<Certificate>> GetCertificatesToBePrinted(int batchNumber);
        Task Update(Batch batch, ICollector<string> StorageQueue);
    }
}
