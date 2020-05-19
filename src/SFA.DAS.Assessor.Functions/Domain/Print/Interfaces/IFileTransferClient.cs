using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IFileTransferClient
    {
        void Send(MemoryStream memoryStream, string fileName);
        Task LogUploadDirectory();
        Task<List<string>> GetListOfDownloadedFiles();
        string DownloadFile(string filename);
        void DeleteFile(string filename);
    }
}
