using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Interfaces
{
    public interface IMessageCommand
    {
        Task<List<string>> Execute(string message);
    }
}
