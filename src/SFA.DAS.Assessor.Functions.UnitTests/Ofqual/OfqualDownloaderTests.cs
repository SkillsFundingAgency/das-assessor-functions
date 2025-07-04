using System.Net.Http;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions.Functions.Ofqual;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofqual
{
    public class OfqualDownloaderTests
    {
        [Test]
        public void OrganisationsConstructor_CreatesOrganisationsHttpClient()
        {
            var mockBlobTransferClient = new Mock<IOfqualDownloadsBlobFileTransferClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<OrganisationsDownloader>>();

            new OrganisationsDownloader(mockBlobTransferClient.Object, mockHttpClientFactory.Object, mockLogger.Object);

            mockHttpClientFactory.Verify(m => m.CreateClient("Organisations"), Times.Once());
        }

        [Test]
        public void QualificationsConstructor_CreatesQualificationsHttpClient()
        {
            var mockBlobTransferClient = new Mock<IOfqualDownloadsBlobFileTransferClient>();
            var mockHttpClient = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<QualificationsDownloader>>();

            new QualificationsDownloader(mockBlobTransferClient.Object, mockHttpClient.Object, mockLogger.Object);

            mockHttpClient.Verify(m => m.CreateClient("Qualifications"), Times.Once());
        }
    }
}
