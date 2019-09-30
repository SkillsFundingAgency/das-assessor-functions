using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SFA.DAS.Assessor.Functions.OpportunityFinder
{
    public class OpportunityFinderDataSync
    {
        private readonly AssessorApiAuthentication assessorApiAuthenticationOptions;

        public OpportunityFinderDataSync(IOptions<AssessorApiAuthentication> assessorApiAuthenticationOptions)
        {
            this.assessorApiAuthenticationOptions = assessorApiAuthenticationOptions.Value;
        }

        [FunctionName("OpportunityFinderDataSync")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
