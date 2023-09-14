using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Functions.Ofqual;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofqual
{
    public class OfqualImportFunctionTests
    {
        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_DownloadOrganisationsData()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync<string>(nameof(OrganisationsDownloader.DownloadOrganisationsData), null), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_ReadOrganisationsData_With_ReturnResultOf_DownloadOrganisationsData()
        {
            const string path = "SomeDirectory/SomeFile.csv";

            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(c => c.CallActivityAsync<string>(nameof(OrganisationsDownloader.DownloadOrganisationsData), null))
                       .ReturnsAsync(path);

            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync<IEnumerable<OfqualOrganisation>>(nameof(OfqualDataReader.ReadOrganisationsData), path), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_InsertOrganisationsDataIntoStaging_With_ReturnResultOf_ReadOrganisationsData()
        {
            var organisations = new List<OfqualOrganisation>();

            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(c => c.CallActivityAsync<IEnumerable<OfqualOrganisation>>(nameof(OfqualDataReader.ReadOrganisationsData), It.IsAny<string>()))
                       .ReturnsAsync(organisations);

            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync<int>(nameof(OrganisationsStager.InsertOrganisationsDataIntoStaging), organisations), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_MoveOfqualFileToProcessed_With_FilePath_ReturnedFrom_DownloadOrganisationsData()
        {
            const string path = "SomeDirectory/SomeFile.csv";

            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(c => c.CallActivityAsync<string>(nameof(OrganisationsDownloader.DownloadOrganisationsData), null))
                       .ReturnsAsync(path);

            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync(nameof(OfqualFileMover.MoveOfqualFileToProcessed), path), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_DownloadQualificationsData()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync<string>(nameof(QualificationsDownloader.DownloadQualificationsData), null), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_ReadQualificationsData_With_ReturnResultOf_DownloadQualificationsData()
        {
            const string path = "SomeDirectory/SomeFile.csv";

            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(c => c.CallActivityAsync<string>(nameof(QualificationsDownloader.DownloadQualificationsData), null))
                       .ReturnsAsync(path);

            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync<IEnumerable<OfqualStandard>>(nameof(OfqualDataReader.ReadQualificationsData), path), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_InsertQualificationsDataIntoStaging_With_ReturnResultOf_ReadQualificationsData()
        {
            var qualifications = new List<OfqualStandard>();

            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(c => c.CallActivityAsync<IEnumerable<OfqualStandard>>(nameof(OfqualDataReader.ReadQualificationsData), It.IsAny<string>()))
                       .ReturnsAsync(qualifications);

            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync<int>(nameof(QualificationsStager.InsertQualificationsDataIntoStaging), qualifications), Times.Once());
        }

        [Test]
        public async Task RunOfqualImportOrchestrator_Calls_MoveOfqualFileToProcessed_With_FilePath_ReturnedFrom_DownloadQualificationsData()
        {
            const string path = "SomeDirectory/SomeFile.csv";

            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(c => c.CallActivityAsync<string>(nameof(QualificationsDownloader.DownloadQualificationsData), null))
                       .ReturnsAsync(path);

            var sut = new OfqualImportFunction();

            await sut.RunOfqualImportOrchestrator(contextMock.Object, new Mock<ILogger>().Object);

            contextMock.Verify(c => c.CallActivityAsync(nameof(OfqualFileMover.MoveOfqualFileToProcessed), path), Times.Once());
        }
    }
}
