using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Standards
{
    public class StandardSummaryUpdateCommand : IStandardSummaryUpdateCommand
    {
        private readonly ILogger<StandardSummaryUpdateCommand> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public StandardSummaryUpdateCommand(ILogger<StandardSummaryUpdateCommand> logger,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Requesting update for summary data");
            try
            {
                await _assessorServiceApi.UpdateStandardSummary();
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error occurred in StandardSummaryUpdateCommand {ex}");
            }

        }
    }
}
