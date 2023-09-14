using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

            var contextMock = new Mock<IDurableActivityContext>();
            contextMock.Setup(c => c.GetInput<string>())
                       .Returns(testFileName);

            var sut = new OfqualFileMover(fileTransferClientMock.Object); 
            await sut.MoveOfqualFileToProcessed(contextMock.Object, new Mock<ILogger>().Object);
            
            fileTransferClientMock.Verify(f => f.DownloadFile($"Downloads/{testFileName}"), Times.Once);
        }

        [Test]
        public async Task MoveOfqualFileToProcessed_UploadsFile_ToProcessedDirectory()
        {
            const string testFileContent = "some content";

            var fileTransferClientMock = new Mock<IOfqualDownloadsBlobFileTransferClient>();
            fileTransferClientMock.Setup(f => f.DownloadFile($"Downloads/{testFileName}"))
                                  .ReturnsAsync(testFileContent);

            var contextMock = new Mock<IDurableActivityContext>();
            contextMock.Setup(c => c.GetInput<string>()).Returns(testFileName);

            var sut = new OfqualFileMover(fileTransferClientMock.Object);
            await sut.MoveOfqualFileToProcessed(contextMock.Object, new Mock<ILogger>().Object);

            fileTransferClientMock.Verify(f => f.UploadFile(testFileContent, $"Processed/{testFileName}"), Times.Once);
        }

        [Test]
        public async Task MoveOfqualFileToProcessed_DeletesDownloadedFile()
        {
            var fileTransferClientMock = new Mock<IOfqualDownloadsBlobFileTransferClient>();

            var contextMock = new Mock<IDurableActivityContext>();
            contextMock.Setup(c => c.GetInput<string>())
                       .Returns(testFileName);

            var sut = new OfqualFileMover(fileTransferClientMock.Object);
            await sut.MoveOfqualFileToProcessed(contextMock.Object, new Mock<ILogger>().Object);

            fileTransferClientMock.Verify(f => f.DeleteFile($"Downloads/{testFileName}"), Times.Once);
        }
    }
}
