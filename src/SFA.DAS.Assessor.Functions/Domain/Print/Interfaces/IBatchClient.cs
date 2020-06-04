using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IBatchClient
    {
        Task<Batch> Get(int batchNumber);
        Task Save(Batch batch);
        Task<int> NextBatchId();
    }
}
