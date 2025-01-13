using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public abstract class OfqualStager
    {
        private readonly IAssessorServiceRepository _assessorServiceRepository;
        private readonly OfqualDataType _ofqualDataType;

        protected OfqualStager(IAssessorServiceRepository assessorServiceRepository, OfqualDataType ofqualDataType)
        {
            _assessorServiceRepository = assessorServiceRepository;
            _ofqualDataType = ofqualDataType;
        }

        public async Task<int> InsertDataIntoStagingTable([ActivityTrigger] IEnumerable<IOfqualRecord> records, ILogger logger)
        {
            logger.LogInformation($"Begin staging of Ofqual {_ofqualDataType} data file.");
            logger.LogInformation($"Checking for downloaded {_ofqualDataType} data file...");

            logger.LogInformation($"{records.Count()} records found. Deleting all existing records in {_ofqualDataType} staging table.");

            int oldRecordsDeleted = await _assessorServiceRepository.ClearOfqualStagingTable(_ofqualDataType);

            logger.LogInformation($"{oldRecordsDeleted} records deleted. Inserting new data into {_ofqualDataType} staging table.");

            int rowsInserted = _assessorServiceRepository.InsertIntoOfqualStagingTable(records, _ofqualDataType);

            logger.LogInformation($"{rowsInserted} rows were staged."   );

            return rowsInserted;
        }
    }
}
