﻿using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class NullFileTransferClient : IFileTransferClient
    {
        public void DeleteFile(string filename)
        {
            return;
        }

        public string DownloadFile(string filename)
        {
            return string.Empty;
        }

        public Task<List<string>> GetListOfDownloadedFiles()
        {
            return Task.FromResult(new List<string>());
        }

        public Task LogUploadDirectory()
        {
            return Task.CompletedTask;
        }

        public void Send(MemoryStream memoryStream, string fileName)
        {
            return;
        }
    }
}