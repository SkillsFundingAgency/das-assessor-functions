using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;

namespace SFA.DAS.Assessor.Functions.OpportunityFinder
{
    public class OpportunityFinderDataSync
    {
        private readonly IAssessorServiceApiClient _assessorApiClient;

        public OpportunityFinderDataSync(IAssessorServiceApiClient assessorServiceApiClient)
        {
            _assessorApiClient = assessorServiceApiClient;
        }

        [FunctionName("OpportunityFinderDataSync")]
        public async Task Run([TimerTrigger("0 0 7 * * *", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Update standard summary timer trigger is running later than scheduled");
                }

                log.LogInformation($"Update standard summary function started");
                log.LogInformation($"Using api base address: {_assessorApiClient.BaseAddress()}");

//                await _assessorApiClient.UpdateStandardSummary();

                log.LogInformation("Update standard summary function completed");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Update standard summary function failed");
            }
        }
    }
}
