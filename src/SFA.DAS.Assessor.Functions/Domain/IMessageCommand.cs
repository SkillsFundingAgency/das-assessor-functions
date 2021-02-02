using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IMessageCommand<TInput, TOutput>
    {
        Task<List<TOutput>> Execute(TInput message);
    }
}
