using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Functions.Ofqual;
using Moq;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using System.Net.Http;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofqual
{
    public class OfqualDownloaderTests
    {
        [Test]
        public void OrganisationsConstructor_CreatesOrganisationsHttpClient()
        {
            var mockBlobTransferClient = new Mock<IOfqualDownloadsBlobFileTransferClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var unused = new OrganisationsDownloader(mockBlobTransferClient.Object, mockHttpClientFactory.Object);

            mockHttpClientFactory.Verify(m => m.CreateClient("Organisations"), Times.Once());
        }

        [Test]
        public void QualificationsConstructor_CreatesQualificationsHttpClient()
        {
            var mockBlobTransferClient = new Mock<IOfqualDownloadsBlobFileTransferClient>();
            var mockHttpClient = new Mock<IHttpClientFactory>();

            var unused = new QualificationsDownloader(mockBlobTransferClient.Object, mockHttpClient.Object);

            mockHttpClient.Verify(m => m.CreateClient("Qualifications"), Times.Once());
        }
    }
}
