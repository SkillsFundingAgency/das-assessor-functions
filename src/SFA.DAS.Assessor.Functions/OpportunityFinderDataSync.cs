using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ApiClient;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;

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
        public void Run([TimerTrigger("0 0 7 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Update standard summary timer trigger is running later than scheduled");
                }

                log.LogInformation($"Update standard summary function started");

                _assessorApiClient.UpdateStandardSummary();

                log.LogInformation("Update standard summary function completed");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Update standard summary function failed");
            }
        }
    }
}
