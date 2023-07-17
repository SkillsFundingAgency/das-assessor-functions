using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.Functions.Assessors;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Assessors
{
    public class AssessmentOrganisationsListUpdateCommand
    {
        private readonly ILogger<AssessmentOrganisationsListUpdateCommand> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public AssessmentOrganisationsListUpdateCommand(ILogger<AssessmentOrganisationsListUpdateCommand> logger,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Requesting update for assessment organisations list");
            await _assessorServiceApi.UpdateAssessmentOrganisationsList();
        }
    }
}
