using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Async;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class FileTransferClient : IFileTransferClient
    {
        private readonly ILogger<FileTransferClient> _logger;
        private readonly SftpSettings _sftpSettings;
        
        private readonly Object _lock = new Object();

        public FileTransferClient(
            ILogger<FileTransferClient> logger,
            IOptions<SftpSettings> options)
        {
            _logger = logger;
            _sftpSettings = options?.Value;
        }

        public void Send(MemoryStream memoryStream, string fileName)
        {
            lock (_lock)
            {
                using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                    Convert.ToInt32(_sftpSettings.Port),
                    _sftpSettings.Username,
                    _sftpSettings.Password))
                {
                    sftpClient.Connect();

                    memoryStream.Position = 0; // ensure memory stream is set to begining of stream          

                    _logger.Log(LogLevel.Information, $"Uploading file ... {_sftpSettings.UploadDirectory}/{fileName}");
                    sftpClient.UploadFile(memoryStream, $"{_sftpSettings.UploadDirectory}/{fileName}");

                    _logger.Log(LogLevel.Information, $"Validating Upload length of file ... {_sftpSettings.UploadDirectory}/{fileName} = {memoryStream.Length}");
                    ValidateUpload(sftpClient, fileName, memoryStream.Length);

                    _logger.Log(LogLevel.Information, $"Validated the upload ...");
                }
            }
        }

        public async Task LogUploadDirectory()
        {
            using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                Convert.ToInt32(_sftpSettings.Port),
                _sftpSettings.Username,
                _sftpSettings.Password))
            {
                sftpClient.Connect();

                var fileList = await sftpClient.ListDirectoryAsync($"{_sftpSettings.UploadDirectory}");
                var fileDetails = new StringBuilder();
                foreach (var file in fileList)
                {
                    fileDetails.Append(file + "\r\n");
                }

                if (fileDetails.Length > 0)
                    _logger.Log(LogLevel.Information, $"Uploaded Files to {_sftpSettings.UploadDirectory} Contains\r\n{fileDetails}");
            }
        }

        public async Task<List<string>> GetListOfDownloadedFiles()
        {
            using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                Convert.ToInt32(_sftpSettings.Port),
                _sftpSettings.Username,
                _sftpSettings.Password))
            {
                sftpClient.Connect();
                var fileList = await sftpClient.ListDirectoryAsync($"{_sftpSettings.ProofDirectory}");
                return fileList.Where(f => !f.IsDirectory).Select(file => file.Name).ToList();
            }
        }

        public string DownloadFile(string fileName)
        {
            var fileContent = string.Empty;
            var fileToDownload = $"{_sftpSettings.ProofDirectory}/{fileName}";

            _logger.Log(LogLevel.Information, $"Connection = {_sftpSettings.RemoteHost}");
            _logger.Log(LogLevel.Information, $"Port = {_sftpSettings.Port}");
            _logger.Log(LogLevel.Information, $"Username = {_sftpSettings.Username}");
            _logger.Log(LogLevel.Information, $"Upload Directory = {_sftpSettings.UploadDirectory}");
            _logger.Log(LogLevel.Information, $"Proof Directory = {_sftpSettings.ProofDirectory}");
            _logger.Log(LogLevel.Information, $"FileName = {fileToDownload}");

            lock (_lock)
            {
                using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                    Convert.ToInt32(_sftpSettings.Port),
                    _sftpSettings.Username,
                    _sftpSettings.Password))
                {
                    sftpClient.Connect();

                    _logger.Log(LogLevel.Information, $"Downloading file ... {fileToDownload}");

                    using (var stream = new MemoryStream())
                    {
                        sftpClient.DownloadFile(fileToDownload, stream);
                        stream.Position = 0;
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            fileContent = reader.ReadToEnd();
                        }
                    }
                }
            }

            return fileContent;
        }

        public void DeleteFile(string filename)
        {
            var fileToDelete = $"{_sftpSettings.ProofDirectory}/{filename}";
            _logger.Log(LogLevel.Information, $"Deleting successfully processed file [{fileToDelete}]");

            lock (_lock)
            {
                using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                    Convert.ToInt32(_sftpSettings.Port),
                    _sftpSettings.Username,
                    _sftpSettings.Password))
                {
                    sftpClient.Connect();
                    sftpClient.DeleteFile(fileToDelete);
                    sftpClient.Disconnect();
                    _logger.Log(LogLevel.Information, $"Deleted successfully processed file [{fileToDelete}]");
                }
            }
        }

        private void ValidateUpload(SftpClient sftpClient, string fileName, long length)
        {
            using (var memoryStreamBack = new MemoryStream())
            {
                sftpClient.DownloadFile($"{_sftpSettings.UploadDirectory}/{fileName}",
                    memoryStreamBack);
                memoryStreamBack.Position = 0;

                if (memoryStreamBack.Length != length)
                {
                    _logger.Log(LogLevel.Information, $"There has been  problem with the sftp file transfer with file name {_sftpSettings.UploadDirectory}/{fileName}");
                    throw new ApplicationException(
                        $"There has been  problem with the sftp file transfer with file name {_sftpSettings.UploadDirectory}/{fileName}");
                }
            }
        }
    }
}
