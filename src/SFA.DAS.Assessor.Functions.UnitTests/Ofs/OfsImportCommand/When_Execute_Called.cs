using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofs;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Types;
using SFA.DAS.AssessorService.Functions.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofs.OfsImportCommand
{
    public class When_Execute_Called
    {
        private Domain.Ofs.OfsImportCommand _sut;
        private Mock<IOfsRegisterApiClient> _ofsRegisterApiClient;
        private Mock<IAssessorServiceRepository> _assessorServiceRepository;
        private Mock<IUnitOfWork> _unitOfWork;

        private List<OfsProvider> _providers;

        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.Ofs.OfsImportCommand>>();
            _ofsRegisterApiClient = new Mock<IOfsRegisterApiClient>();
            _assessorServiceRepository = new Mock<IAssessorServiceRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();

            // Arrange
            _providers = new List<OfsProvider>
            {
                new OfsProvider { Ukprn = "12345678", RegistrationStatus = "Registered", HighestLevelOfDegreeAwardingPowers = "Not applicable" },
                new OfsProvider { Ukprn = "23456781", RegistrationStatus = "Registgered", HighestLevelOfDegreeAwardingPowers = "Taught" }
            };

            _ofsRegisterApiClient
                .Setup(p => p.GetProviders())
                .ReturnsAsync(_providers);

            _sut = new Domain.Ofs.OfsImportCommand(logger.Object, _ofsRegisterApiClient.Object, _assessorServiceRepository.Object, _unitOfWork.Object);
        }

        [Test]
        public async Task Then_GetProviders_ShouldBeCalled()
        {
            // Act
            await _sut.Execute();

            // Assert
            _ofsRegisterApiClient.Verify(p => p.GetProviders(), Times.Once());
        }

        [Test]
        public async Task Then_ClearStagingOfsOrganisationsTable_ShouldBeCalled()
        {
            // Act
            await _sut.Execute();

            // Assert
            _assessorServiceRepository.Verify(p => p.ClearStagingOfsOrganisationsTable(), Times.Once());
        }

        [Test]
        public async Task Then_InsertIntoStagingOfsOrganisationTable_ShouldBeCalled()
        {
            List<OfsOrganisation> passedOrganisations = null;
            _assessorServiceRepository.Setup(p => p.InsertIntoStagingOfsOrganisationTable(It.IsAny<IEnumerable<OfsOrganisation>>()))
                .Callback<IEnumerable<OfsOrganisation>>(orgs => passedOrganisations = orgs.ToList());

            // Act
            await _sut.Execute();

            // Assert
            _assessorServiceRepository.Verify(p => p.InsertIntoStagingOfsOrganisationTable(It.IsAny<List<OfsOrganisation>>()), Times.Once());

            Assert.IsNotNull(passedOrganisations);
            Assert.AreEqual(2, passedOrganisations.Count);
        }

        [Test]
        public async Task Then_LoadOfsStandards_ShouldBeCalled()
        {
            // Act
            await _sut.Execute();

            // Assert
            _assessorServiceRepository.Verify(p => p.LoadOfsStandards(), Times.Once());
        }
    }
}
