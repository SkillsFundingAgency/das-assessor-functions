using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IQueueCommand
    {
        Task<List<string>> Execute();
    }
}
