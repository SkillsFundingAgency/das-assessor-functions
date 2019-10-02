using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Infrastructure;

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
