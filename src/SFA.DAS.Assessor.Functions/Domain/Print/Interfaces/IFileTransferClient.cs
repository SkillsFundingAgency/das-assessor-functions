using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IFileTransferClient
    {
        void Send(MemoryStream memoryStream, string file);
        Task<List<string>> GetFileNames(string directory);
        Task<List<string>> GetFileNames(string directory, string pattern);
        string DownloadFile(string file);
        void DeleteFile(string file);
        void MoveFileToArchive(string remotefileName, string destinationDirectory);
    }
}
