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
        private readonly ILogger<OfqualStager> _logger;

        protected OfqualStager(IAssessorServiceRepository assessorServiceRepository, OfqualDataType ofqualDataType, ILogger<OfqualStager> logger)
        {
            _assessorServiceRepository = assessorServiceRepository;
            _ofqualDataType = ofqualDataType;
            _logger = logger;
        }

        public async Task<int> InsertDataIntoStagingTable([ActivityTrigger] IEnumerable<IOfqualRecord> records)
        {
            _logger.LogInformation($"Begin staging of Ofqual {_ofqualDataType} data file.");
            _logger.LogInformation($"Checking for downloaded {_ofqualDataType} data file...");

            _logger.LogInformation($"{records.Count()} records found. Deleting all existing records in {_ofqualDataType} staging table.");

            int oldRecordsDeleted = await _assessorServiceRepository.ClearOfqualStagingTable(_ofqualDataType);

            _logger.LogInformation($"{oldRecordsDeleted} records deleted. Inserting new data into {_ofqualDataType} staging table.");

            int rowsInserted = _assessorServiceRepository.InsertIntoOfqualStagingTable(records, _ofqualDataType);

            _logger.LogInformation($"{rowsInserted} rows were staged."   );

            return rowsInserted;
        }
    }
}
