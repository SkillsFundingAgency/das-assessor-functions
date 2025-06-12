using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessments.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Assessments
{
    public class AssessmentsSummaryUpdateFunction
    {
        private readonly IAssessmentsSummaryUpdateCommand _command;

        public AssessmentsSummaryUpdateFunction(IAssessmentsSummaryUpdateCommand command)
        {
            _command = command;
        }

        [Function("AssessmentsSummaryUpdate")]
        public async Task Run([TimerTrigger("%AssessmentsSummaryUpdateOptions%", RunOnStartup = false)]TimerInfo myTimer, ILogger log)
        {
            await FunctionHelper.Run("AssessmentsSummaryUpdate", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, log);
        }
    }
}
