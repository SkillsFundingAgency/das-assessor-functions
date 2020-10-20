using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IFileTransferClient
    {
        string ContainerName { get; set; }
        Task<List<string>> GetFileNames(string directory, bool recursive);
        Task<List<string>> GetFileNames(string directory, string pattern, bool recursive);
        Task UploadFile(MemoryStream memoryStream, string path);
        Task<string> DownloadFile(string path);
        Task DeleteFile(string path);
        Task MoveFile(string sourcePath, IFileTransferClient destinationFileTransferClient, string destinationPath);
    }
}
