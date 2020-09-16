using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Functions.ExternalApiDataSync
{
    public class ExternalApiDataSyncFunction
    {        
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public ExternalApiDataSyncFunction(IAssessorServiceApiClient assessorServiceApi)
        {            
            _assessorServiceApi = assessorServiceApi;
        }
        
        [FunctionName("ExternalApiDataSyncFunction")]
        public async Task Run([TimerTrigger("%ExternalApiDataSyncFunctionSchedule%", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            var dataSyncRequest = new GetDataSyncRequest();
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao ExternalApiDataSyncFunction timer trigger is running later than scheduled");
                }

                log.LogInformation($"Epao ExternalApiDataSyncFunction started");

                await _assessorServiceApi.ExternalApiDataSync(dataSyncRequest);

                log.LogInformation("Epao ExternalApiDataSyncFunction function completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Epao ExternalApiDataSyncFunction function failed");
            }
        }
    }
}