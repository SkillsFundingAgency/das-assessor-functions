using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;

namespace SFA.DAS.Assessor.Functions.UnitTests.DatabaseMaintenance
{
    public class When_Execute_Called
    {
        private Domain.DatabaseMaintenance.DatabaseMaintenanceCommand _sut;
        private Mock<IAssessorServiceRepository> _mockAssessorServiceRepository;
        private Mock<IOptions<DatabaseMaintenanceOptions>> _mockOptions;
        
        [SetUp]
        public void Arrange()
        {
            var logger = new Mock<ILogger<Domain.DatabaseMaintenance.DatabaseMaintenanceCommand>>();

            _mockAssessorServiceRepository = new Mock<IAssessorServiceRepository>();
            _mockAssessorServiceRepository
                .Setup(p => p.DatabaseMaintenance())
                .ReturnsAsync(new List<string> { "result" });
            
            _mockOptions = new Mock<IOptions<DatabaseMaintenanceOptions>>();
            _mockOptions
                .Setup(p => p.Value)
                .Returns(new DatabaseMaintenanceOptions() { Enabled = true });

            _sut = new Domain.DatabaseMaintenance.DatabaseMaintenanceCommand(_mockOptions.Object, _mockAssessorServiceRepository.Object, logger.Object);
        }

        [Test]
        public async Task ThenItShouldCallDatabaseMaintenance()
        {
            // Act
            await _sut.Execute();

            // Assert
            _mockAssessorServiceRepository.Verify(p => p.DatabaseMaintenance(), Times.Once);
        }
    }
}
