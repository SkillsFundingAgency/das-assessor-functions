using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IBatchService
    {
        Task<Batch> Get(int batchNumber);
        Task<ValidationResponse> Save(Batch batch);
        Task<int> NextBatchId();
    }
}
