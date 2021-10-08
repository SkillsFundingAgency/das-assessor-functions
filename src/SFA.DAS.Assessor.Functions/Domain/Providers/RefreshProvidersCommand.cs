using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.Providers;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Learners
{
    public class RefreshProvidersCommand : IRefreshProvidersCommand
    {
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        private readonly ILogger<ImportLearnersCommand> _logger;
        private readonly RefreshProvidersOptions _options;

        public RefreshProvidersCommand(IAssessorServiceApiClient assessorServiceApi,
            ILogger<ImportLearnersCommand> logger, IOptions<RefreshProvidersOptions> options)
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
                    _logger.LogInformation($"RefreshProvidersCommand cannot be started, it is not enabled");
                    return;
                }

                _logger.LogInformation($"RefreshProvidersCommand started");

                await _assessorServiceApi.RefreshProviders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RefreshProvidersCommand failed");
                throw;
            }
        }
    }
}
