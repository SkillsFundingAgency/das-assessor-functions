using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessments.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Assessments
{
    public class AssessmentsSummaryUpdateCommand : IAssessmentsSummaryUpdateCommand
    {
        private readonly ILogger<AssessmentsSummaryUpdateCommand> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public AssessmentsSummaryUpdateCommand(ILogger<AssessmentsSummaryUpdateCommand> logger,
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
                await _assessorServiceApi.UpdateAssessmentsSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in AssessmentsSummaryUpdateCommand {ex}");
            }

        }
    }
}
