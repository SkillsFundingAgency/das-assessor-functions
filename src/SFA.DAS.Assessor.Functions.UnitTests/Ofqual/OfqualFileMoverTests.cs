using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions.Functions.Ofqual;

namespace SFA.DAS.Assessor.Functions.UnitTests.Ofqual
{
    public class OfqualFileMoverTests
    {
        private const string testFileName = "testfile.csv";

        [Test]
        public async Task MoveOfqualFileToProcessed_DownloadsFile_ToDownloadsDirectory()
        {
            var fileTransferClientMock = new Mock<IOfqualDownloadsBlobFileTransferClient>();

            var sut = new OfqualFileMover(fileTransferClientMock.Object, new Mock<ILogger<OfqualFileMover>>().Object); 
            await sut.MoveOfqualFileToProcessed(testFileName);
            
            fileTransferClientMock.Verify(f => f.DownloadFile($"Downloads/{testFileName}"), Times.Once);
        }

        [Test]
        public async Task MoveOfqualFileToProcessed_UploadsFile_ToProcessedDirectory()
        {
            const string testFileContent = "some content";

            var fileTransferClientMock = new Mock<IOfqualDownloadsBlobFileTransferClient>();
            fileTransferClientMock.Setup(f => f.DownloadFile($"Downloads/{testFileName}"))
                                  .ReturnsAsync(testFileContent);

            var sut = new OfqualFileMover(fileTransferClientMock.Object, new Mock<ILogger<OfqualFileMover>>().Object);
            await sut.MoveOfqualFileToProcessed(testFileName);

            fileTransferClientMock.Verify(f => f.UploadFile(testFileContent, $"Processed/{testFileName}"), Times.Once);
        }

        [Test]
        public async Task MoveOfqualFileToProcessed_DeletesDownloadedFile()
        {
            var fileTransferClientMock = new Mock<IOfqualDownloadsBlobFileTransferClient>();

            var sut = new OfqualFileMover(fileTransferClientMock.Object, new Mock<ILogger<OfqualFileMover>>().Object);
            await sut.MoveOfqualFileToProcessed(testFileName);

            fileTransferClientMock.Verify(f => f.DeleteFile($"Downloads/{testFileName}"), Times.Once);
        }
    }
}
