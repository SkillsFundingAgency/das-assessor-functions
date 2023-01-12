using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class DequeueLearnerInfoCommand : IDequeueLearnerInfoCommand
    {
        private readonly IAssessorServiceRepository _assessorServiceRepository;
        private readonly ILogger<DequeueLearnerInfoCommand> _logger;

        public DequeueLearnerInfoCommand(ILogger<DequeueLearnerInfoCommand> logger, IAssessorServiceRepository assessorServiceRepository)
        {
            _logger = logger;
            _assessorServiceRepository = assessorServiceRepository;
        }

        public async Task Execute(string message)
        {
            _logger.LogInformation("DequeueLearnerInfoCommand started");

            var learner = JsonConvert.DeserializeObject<UpdateLearnersInfoMessage>(message);

            _logger.LogInformation($"Started processing learner uln {learner.Uln} StdCode {learner.StdCode} EmployerAccountId {learner.EmployerAccountId} EmployerName {learner.EmployerName} employer information");

            var affectedRows = await _assessorServiceRepository.UpdateLearnerInfo((learner.Uln, learner.StdCode, learner.EmployerAccountId, learner.EmployerName));

            _logger.LogInformation($"DequeueLearnerInfoCommand affected Rows Count {affectedRows}");

            _logger.LogInformation("DequeueLearnerInfoCommand Completed");
        }
    }
}
