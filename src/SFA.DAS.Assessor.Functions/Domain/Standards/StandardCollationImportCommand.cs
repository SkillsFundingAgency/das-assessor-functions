using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Standards
{
    public class StandardCollationImportCommand : IStandardCollationImportCommand
    {
        private readonly ILogger<StandardCollationImportCommand> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        
        public StandardCollationImportCommand(ILogger<StandardCollationImportCommand> logger,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Requesting import for standard collation data");
            await _assessorServiceApi.UpdateStandards();
        }
    }
}
