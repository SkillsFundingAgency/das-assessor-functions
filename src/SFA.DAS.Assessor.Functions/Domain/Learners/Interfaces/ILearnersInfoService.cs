using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface ILearnersInfoService
    {
        Task<List<UpdateLearnersInfoMessage>> GetLearnersToUpdate();
        Task<List<UpdateLearnersInfoMessage>> ProcessLearners(List<UpdateLearnersInfoMessage> learnersInfoMessages);
    }
}