using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.Learners;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class ImportLearnersCommand : IImportLearnersCommand
    {
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        private readonly ILogger<ImportLearnersCommand> _logger;
        private readonly ImportLearnersOptions _options;

        public ImportLearnersCommand(IAssessorServiceApiClient assessorServiceApi,
            ILogger<ImportLearnersCommand> logger, IOptions<ImportLearnersOptions> options)
        {
            _assessorServiceApi = assessorServiceApi;
            _logger = logger;
            _options = options?.Value;
        }

        public async Task Execute()
        {
            try
            {
                if (!_options.Enabled)
                {
                    _logger.LogInformation($"ImportLearnersCommand cannot be started, it is not enabled");
                    return;
                }

                _logger.LogInformation($"ImportLearnersCommand started");

                await _assessorServiceApi.ImportLearners();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ImportLearnersCommand failed");
                throw;
            }
        }
    }
}
