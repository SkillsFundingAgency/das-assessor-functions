using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Assessors
{
    public class AparSummaryUpdateCommand
    {
        private readonly ILogger<AparSummaryUpdateCommand> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public AparSummaryUpdateCommand(ILogger<AparSummaryUpdateCommand> logger,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Requesting update for APAR summary");
            await _assessorServiceApi.AparSummaryUpdate();
        }
    }
}
