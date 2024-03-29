﻿using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Standards
{
    public class StandardImportCommand : IStandardImportCommand
    {
        private readonly ILogger<StandardImportCommand> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;
        
        public StandardImportCommand(ILogger<StandardImportCommand> logger,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Requesting import for standard data");
            await _assessorServiceApi.UpdateStandards();
        }
    }
}
