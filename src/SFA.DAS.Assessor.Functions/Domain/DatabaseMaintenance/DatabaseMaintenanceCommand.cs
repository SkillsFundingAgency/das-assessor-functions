using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;

namespace SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance
{
    public class DatabaseMaintenanceCommand : IDatabaseMaintenanceCommand
    {
        private readonly DatabaseMaintenanceOptions _options;
        
        private readonly IAssessorServiceRepository _assessorServiceRepository;
        private readonly ILogger<DatabaseMaintenanceCommand> _logger;
        
        public DatabaseMaintenanceCommand(IOptions<DatabaseMaintenanceOptions> options, IAssessorServiceRepository databaseMaintanenceRepository, ILogger<DatabaseMaintenanceCommand> logger)
        {
            _options = options?.Value;
            _assessorServiceRepository = databaseMaintanenceRepository;
            _logger = logger;
        }

        public async Task Execute()
        {
            if(_options.Enabled)
            {
                _logger.LogInformation("Performing database maintenance");

                var results = await _assessorServiceRepository.DatabaseMaintenance();
                var logMessage = string.Join(", ", results.ToArray());
                
                _logger.LogInformation($"Database maintenance results: {logMessage}");
            }
            else
            {
                _logger.LogInformation("Database maintenance is not enabled");
            }
        }
    }
}
