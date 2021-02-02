using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IQueueCommand<TOutput>
    {
        Task<List<TOutput>> Execute();
    }
}
