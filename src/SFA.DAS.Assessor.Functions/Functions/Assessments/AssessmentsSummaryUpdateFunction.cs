using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Assessments.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Assessments
{
    public class AssessmentsSummaryUpdateFunction
    {
        private readonly IAssessmentsSummaryUpdateCommand _command;
        private readonly ILogger<AssessmentsSummaryUpdateFunction> _logger;

        public AssessmentsSummaryUpdateFunction(IAssessmentsSummaryUpdateCommand command, ILogger<AssessmentsSummaryUpdateFunction> logger)
        {
            _command = command;
            _logger = logger;
        }

        [Function("AssessmentsSummaryUpdate")]
        public async Task Run([TimerTrigger("%AssessmentsSummaryUpdateSchedule%", RunOnStartup = true)]TimerInfo myTimer)
        {
            await FunctionHelper.Run("AssessmentsSummaryUpdate", async () => 
            { 
                await _command.Execute(); 
            }, myTimer, _logger);
        }
    }
}
