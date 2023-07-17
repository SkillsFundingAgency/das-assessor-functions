using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Assessors
{
    public class AssessmentOrganisationsListUpdateFunction
    {
        private readonly IAssessmentOrganisationsListUpdateCommand _command;

        public AssessmentOrganisationsListUpdateFunction(IAssessmentOrganisationsListUpdateCommand command)
        {
            _command = command;
        }

        [FunctionName("AssessmentOrganisationsListUpdate")]
        public async Task Run([TimerTrigger("%FunctionsOptions:AssessmentOrganisationsListUpdateOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation($"AssessmentOrganisationsListUpdate has started later than scheduled");
                }
                else
                {
                    log.LogInformation($"AssessmentOrganisationsListUpdate has started");
                }

                await _command.Execute();

                log.LogInformation("AssessmentOrganisationsListUpdate has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "AssessmentOrganisationsListUpdate has failed");
            }
        }
    }
}
