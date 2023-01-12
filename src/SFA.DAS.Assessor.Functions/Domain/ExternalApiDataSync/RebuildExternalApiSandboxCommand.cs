using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.ExternalApiDataSync;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync
{
    public class RebuildExternalApiSandboxCommand : IRebuildExternalApiSandboxCommand
    {
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        private readonly ILogger<RebuildExternalApiSandboxCommand> _logger;
        private readonly RebuildExternalApiSandboxOptions _options;

        public RebuildExternalApiSandboxCommand(IAssessorServiceApiClient assessorServiceApi,
            ILogger<RebuildExternalApiSandboxCommand> logger, IOptions<RebuildExternalApiSandboxOptions> options)
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
                    _logger.LogInformation($"RebuildExternalApiSandboxCommand cannot be started, it is not enabled");
                    return;
                }

                _logger.LogInformation($"RebuildExternalApiSandboxCommand started");

                await _assessorServiceApi.RebuildExternalApiSandbox();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RebuildExternalApiSandboxCommand failed");
                throw;
            }
        }
    }
}