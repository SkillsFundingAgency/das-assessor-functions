using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions.Functions.Ofqual;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofqual
{
    public class OfqualStagerTests
    {
        [Test]
        public async Task OrganisationsStager_InsertDataIntoStagingTable_ClearsOrganisationsStagingTable()
        {
            var assessorServiceRepositoryMock = new Mock<IAssessorServiceRepository>();
            var logger = new Mock<ILogger>();

            var sut = new OrganisationsStager(assessorServiceRepositoryMock.Object);

            await sut.InsertDataIntoStagingTable(new List<OfqualOrganisation>(), logger.Object);

            assessorServiceRepositoryMock.Verify(a => a.ClearOfqualStagingTable(OfqualDataType.Organisations), Times.Once());
        }

        [Test]
        public async Task QualificationsStager_InsertDataIntoStagingTable_ClearsQualificationsStagingTable()
        {
            var assessorServiceRepositoryMock = new Mock<IAssessorServiceRepository>();
            var logger = new Mock<ILogger>();

            var sut = new QualificationsStager(assessorServiceRepositoryMock.Object);

            await sut.InsertDataIntoStagingTable(new List<OfqualStandard>(), logger.Object);

            assessorServiceRepositoryMock.Verify(a => a.ClearOfqualStagingTable(OfqualDataType.Qualifications), Times.Once());
        }
    }
}
