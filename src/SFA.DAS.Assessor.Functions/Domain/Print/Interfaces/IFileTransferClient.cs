using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IFileTransferClient
    {
        string ContainerName { get; set; }
        Task<List<string>> GetFileNames(string directory, bool recursive);
        Task<List<string>> GetFileNames(string directory, string pattern, bool recursive);
        Task UploadFile(string fileContents, string path);
        Task<bool?> FileExists(string path);
        Task<string> DownloadFile(string path);
        Task DeleteFile(string path);
    }
}
