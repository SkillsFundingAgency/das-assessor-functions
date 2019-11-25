using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ApiClient;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;

namespace SFA.DAS.Assessor.Functions.OpportunityFinder
{
    public class EpaoDataSync
    {
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;

        public EpaoDataSync(IDataCollectionServiceApiClient dataCollectionServiceApiClient)
        {
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
        }

        [FunctionName("EpaoDataSync")]
        public async Task Run([TimerTrigger("0 0 7 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao data sync timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao data sync function started");
                log.LogInformation($"Using data collection api base address: {_dataCollectionServiceApiClient.Client.BaseAddress}");

                // sample calls to verify connection to DC API - this will become a testable command pattern
                var ukprn = await _dataCollectionServiceApiClient.GetProviders(new DateTime(2000,1, 1));
                if(ukprn.Count > 0)
                {
                    var learners = await _dataCollectionServiceApiClient.GetLearners(ukprn[0]);
                }

                log.LogInformation("Epao data sync function function completed");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Epao data sync function function failed");
            }
        }
    }
}
