using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofs;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs;
using SFA.DAS.AssessorService.Functions.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ofs
{
    public class OfsImportCommand : IOfsImportCommand
    {
        private readonly ILogger<OfsImportCommand> _logger;
        private readonly IOfsRegisterApiClient _ofsRegisterApiClient;
        private readonly IAssessorServiceRepository _assessorServiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OfsImportCommand(ILogger<OfsImportCommand> logger,
            IOfsRegisterApiClient ofsRegisterApiClient, IAssessorServiceRepository assessorServiceRepository, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _ofsRegisterApiClient = ofsRegisterApiClient;
            _assessorServiceRepository = assessorServiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("Importing Ofs standards started");

                _logger.LogInformation("Importing Ofs standards, getting providers");
                var providers = await _ofsRegisterApiClient.GetProviders();

                _unitOfWork.Begin();

                _logger.LogInformation("Importing Ofs standards, clearing staging for ofs organisations");
                await _assessorServiceRepository.ClearStagingOfsOrganisationsTable();

                _logger.LogInformation("Importing Ofs standards, inserting staging for ofs organisations");
                await _assessorServiceRepository.InsertIntoStagingOfsOrganisationTable(providers.Select(p => (OfsOrganisation)p).ToList());

                _logger.LogInformation("Importing Ofs standards, loading ofs organisations and standards");
                var standardsImported = await _assessorServiceRepository.LoadOfsStandards();

                _unitOfWork.Commit();

                _logger.LogInformation($"Importing Ofs standards completed, {standardsImported} standards imported");
            }
            catch(Exception ex)
            {
                _unitOfWork.Rollback();
                _logger.LogError(ex, "Importing Ofs standards failed");
                throw;
            }
        }
    }
}
