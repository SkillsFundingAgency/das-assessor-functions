using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Data
{
    public interface IAssessorServiceRepository
    {
        Task<Dictionary<string, long>> GetLearnersWithoutEmployerInfo();
        Task<int> UpdateLearnerInfo((long uln, int standardCode, long employerAccountId, string employerName) learnerInfo);
        int InsertIntoOfqualStagingTable(IEnumerable<IOfqualRecord> recordsToInsert, OfqualDataType ofqualDataType);
        Task<int> ClearOfqualStagingTable(OfqualDataType ofqualDataType);
        Task<int> LoadOfqualStandards();
        Task<int> ClearStagingOfsOrganisationsTable();
        Task<int> InsertIntoStagingOfsOrganisationTable(IEnumerable<OfsOrganisation> recordsToInsert);
        Task<int> LoadOfsStandards();
        Task<List<string>> DatabaseMaintenance();
    }
}
