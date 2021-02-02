using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Data
{
    public interface IDatabaseMaintenanceRepository
    {
        Task<List<string>> DatabaseMaintenance();
    }
}
