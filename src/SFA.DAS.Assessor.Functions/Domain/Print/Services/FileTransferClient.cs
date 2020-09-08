﻿using Microsoft.Extensions.Logging;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Renci.SshNet.Sftp;

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

        public void Send(MemoryStream memoryStream, string file)
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

                    _logger.Log(LogLevel.Information, $"Uploading file ... {file}");
                    sftpClient.UploadFile(memoryStream, file);

                    _logger.Log(LogLevel.Information, $"Validating Upload length of file ... {file} = {memoryStream.Length}");
                    ValidateUpload(sftpClient, file, memoryStream.Length);

                    _logger.Log(LogLevel.Information, $"Validated the upload ...");
                }
            }
        }

        public void MoveFolderToArchive(string sourceDirectory, string destinationDirectory, string fileName)
        {
            lock (_lock)
            {
                using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                    Convert.ToInt32(_sftpSettings.Port),
                    _sftpSettings.Username,
                    _sftpSettings.Password))
                {
                    sftpClient.Connect();
                    //Get first file in the Directory 
                    _logger.Log(LogLevel.Information, $"Listing Directory ... {sourceDirectory}");
                    SftpFile eachRemoteFile = sftpClient.ListDirectory(sourceDirectory).First();
                    _logger.Log(LogLevel.Information, $"First Remotefile ... {eachRemoteFile}");
                    //Move only file
                    if (eachRemoteFile.IsRegularFile)
                    {
                        bool eachFileExistsInArchive = CheckIfRemoteFileExists(sftpClient, destinationDirectory, eachRemoteFile.Name);

                        //MoveTo will result in error if filename already exists in the target folder. Prevent that error by checking if File name exists
                        string eachFileNameInArchive = eachRemoteFile.Name;

                        if (eachFileExistsInArchive)
                        {
                            //Change file name if the file already exists
                            eachFileNameInArchive = eachFileNameInArchive + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
                        }
                        eachRemoteFile.MoveTo(destinationDirectory + eachFileNameInArchive);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if Remote folder contains the given file name
        /// </summary>
        public bool CheckIfRemoteFileExists(SftpClient sftpClient, string remoteFolderName, string remotefileName)
        {
            bool isFileExists = sftpClient
                .ListDirectory(remoteFolderName)
                .Any(
                    f => f.IsRegularFile &&
                         f.Name.ToLower() == remotefileName.ToLower()
                );
            return isFileExists;
        }

        public async Task<List<string>> GetFileNames(string directory, string pattern)
        {
            var fileList = await GetFileNames(directory);

            return fileList.Where(f => Regex.IsMatch(f, pattern)).ToList();
        }

        public async Task<List<string>> GetFileNames(string directory)
        {
            using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                Convert.ToInt32(_sftpSettings.Port),
                _sftpSettings.Username,
                _sftpSettings.Password))
            {
                sftpClient.Connect();
                var fileList = await sftpClient.ListDirectoryAsync(directory);
                return fileList.Where(f => !f.IsDirectory).Select(file => file.Name).ToList();
            }
        }

        public string DownloadFile(string file)
        {
            var fileContent = string.Empty;

            _logger.Log(LogLevel.Information, $"Connection = {_sftpSettings.RemoteHost}");
            _logger.Log(LogLevel.Information, $"Port = {_sftpSettings.Port}");
            _logger.Log(LogLevel.Information, $"Username = {_sftpSettings.Username}");
            _logger.Log(LogLevel.Information, $"FileName = {file}");

            lock (_lock)
            {
                using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                    Convert.ToInt32(_sftpSettings.Port),
                    _sftpSettings.Username,
                    _sftpSettings.Password))
                {
                    sftpClient.Connect();

                    _logger.Log(LogLevel.Information, $"Downloading file ... {file}");

                    using (var stream = new MemoryStream())
                    {
                        sftpClient.DownloadFile(file, stream);
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

        public void DeleteFile(string file)
        {
            _logger.Log(LogLevel.Information, $"Deleting successfully processed file [{file}]");

            lock (_lock)
            {
                using (var sftpClient = new SftpClient(_sftpSettings.RemoteHost,
                    Convert.ToInt32(_sftpSettings.Port),
                    _sftpSettings.Username,
                    _sftpSettings.Password))
                {
                    sftpClient.Connect();
                    sftpClient.DeleteFile(file);
                    sftpClient.Disconnect();
                    _logger.Log(LogLevel.Information, $"Deleted successfully processed file [{file}]");
                }
            }
        }

        private void ValidateUpload(SftpClient sftpClient, string file, long length)
        {
            using (var memoryStreamBack = new MemoryStream())
            {
                sftpClient.DownloadFile(file, memoryStreamBack);
                memoryStreamBack.Position = 0;

                if (memoryStreamBack.Length != length)
                {
                    _logger.Log(LogLevel.Information, $"There has been  problem with the sftp file transfer with file name {file}");
                    throw new ApplicationException(
                        $"There has been  problem with the sftp file transfer with file name {file}");
                }
            }
        }
    }
}
