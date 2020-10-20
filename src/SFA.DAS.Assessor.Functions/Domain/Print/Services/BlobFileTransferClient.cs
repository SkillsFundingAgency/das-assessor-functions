using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class BlobTransferClient : IFileTransferClient
    {
        private readonly ILogger<BlobTransferClient> _logger;
        private string _connectionString { get; }
        
        public string ContainerName { get; set; }

        public BlobTransferClient(ILogger<BlobTransferClient> logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;
        }

        public async Task<List<string>> GetFileNames(string directory, string pattern, bool recursive)
        {
            var fileList = await GetFileNames(directory, recursive);
            return fileList.Where(f => Regex.IsMatch(f, pattern)).ToList();
        }

        public async Task<List<string>> GetFileNames(string directory, bool recursive)
        {
            var fileNames = new List<string>();

            try
            {
                var blobs = await GetBlobsHierarchicalListingAsync(GetCloudBlobDirectory(directory), recursive);
                fileNames.AddRange(blobs.ConvertAll<string>(p => GetBlobFileName(p.Name)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error listing filenames from {directory}");
            }

            return fileNames;
        }

        public async Task UploadFile(MemoryStream memoryStream, string path)
        {
            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogInformation($"Uploading {path} to blob storage {ContainerName}");

                await blob.UploadFromStreamAsync(memoryStream);

                _logger.LogInformation($"Uploaded {path} to blob storage {ContainerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {path}");
            }
        }

        public async Task<string> DownloadFile(string path)
        {
            var fileContent = string.Empty;

            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogInformation($"Downloading {path} from blob storage {ContainerName}");

                using (var stream = new MemoryStream())
                {
                    await Download(path, stream);
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

                _logger.LogInformation($"Downloaded {path} from blob storage {ContainerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading {path} from blob storage {ContainerName}");
            }

            return fileContent;
        }

        public async Task DeleteFile(string path)
        {
            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogInformation($"Deleting {path} from blob storage {ContainerName}");

                await blob.DeleteAsync();

                _logger.LogInformation($"Deleted {path} from blob storage {ContainerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {path} from blob storage {ContainerName}");
            }
        }

        public async Task MoveFile(string sourcePath, IFileTransferClient destinationFileTransferClient, string destinationPath)
        {
            try
            {
                _logger.LogInformation($"Moving {sourcePath} from blob storage {ContainerName} to {destinationPath} in blob storage {destinationFileTransferClient.ContainerName}");

                using (var stream = new MemoryStream())
                {
                    await Download(sourcePath, stream);
                    await destinationFileTransferClient.UploadFile(stream, destinationPath);
                    await DeleteFile(sourcePath);
                }

                _logger.LogInformation($"Moved {sourcePath} from blob storage {ContainerName} to {destinationPath} in blob storage {destinationFileTransferClient.ContainerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moving {sourcePath} from blob storage {ContainerName} to {destinationPath} in blob storage {destinationFileTransferClient.ContainerName}");
            }
        }

        private async Task Download(string path, MemoryStream stream)
        {
            var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
            var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

            using (var memoryStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memoryStream);
                
                memoryStream.Position = 0;
                memoryStream.CopyTo(stream);
                stream.Position = 0;
            }
        }

        private CloudBlobDirectory GetCloudBlobDirectory(string path)
        {
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(ContainerName);

            var directory = container.GetDirectoryReference(GetBlobDirectoryName(path));
            container.CreateIfNotExists();

            return directory;
        }

        private string GetBlobFileName(string path)
        {
            return Path.GetFileName(path);
        }

        private string GetBlobDirectoryName(string path)
        {
            var directoryName = Path.GetDirectoryName(path);
            
            directoryName = !string.IsNullOrEmpty(directoryName)
                ? directoryName.Replace('\\', '/').TrimStart('/')
                : path;

            return directoryName.EndsWith('/')
                ? directoryName
                : directoryName += '/';
        }

        private static async Task<List<CloudBlob>> GetBlobsHierarchicalListingAsync(CloudBlobDirectory directory, bool recursive)
        {
            var blobs = new List<CloudBlob>();

            try
            {
                BlobContinuationToken continuationToken = null;

                do
                {
                    BlobResultSegment resultSegment = await directory.ListBlobsSegmentedAsync(continuationToken);

                    foreach (var blobItem in resultSegment.Results)
                    {
                        if (blobItem is CloudBlobDirectory && recursive)
                        {
                            var dir = blobItem as CloudBlobDirectory;
                            blobs.AddRange(await GetBlobsHierarchicalListingAsync(dir, recursive));
                        }
                        else if (blobItem is CloudBlob)
                        {
                            blobs.Add(blobItem as CloudBlob);
                        }
                    }

                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return blobs;
        }
    }
}
