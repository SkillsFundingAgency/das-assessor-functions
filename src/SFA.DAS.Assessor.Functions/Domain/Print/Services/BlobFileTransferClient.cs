﻿using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
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
    public class BlobFileTransferClient : IExternalBlobFileTransferClient, IInternalBlobFileTransferClient
    {
        private readonly ILogger<BlobFileTransferClient> _logger;
        private string _connectionString { get; }
        private string _containerName { get; set; }

        public BlobFileTransferClient(ILogger<BlobFileTransferClient> logger, string connectionString, string containerName)
        {
            _logger = logger;
            _connectionString = connectionString;
            _containerName = containerName;
        }

        public string ContainerName => _containerName;

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
                throw;
            }

            return fileNames;
        }

        public async Task UploadFile(string fileContents, string path)
        {
            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogDebug($"Uploading {path} to blob storage {_containerName}");

                byte[] array = Encoding.ASCII.GetBytes(fileContents);
                using (var stream = new MemoryStream(array))
                {
                    await blob.UploadFromStreamAsync(stream);
                }

                _logger.LogDebug($"Uploaded {path} to blob storage {_containerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {path}");
                throw;
            }
        }

        public async Task<string> DownloadFile(string path)
        {
            var fileContent = string.Empty;

            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogDebug($"Downloading {path} from blob storage {_containerName}");

                using (var stream = new MemoryStream())
                {
                    await Download(path, stream);
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

                _logger.LogDebug($"Downloaded {path} from blob storage {_containerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading {path} from blob storage {_containerName}");
                throw;
            }

            return fileContent;
        }

        public async Task DeleteFile(string path)
        {
            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogDebug($"Deleting {path} from blob storage {_containerName}");

                await blob.DeleteAsync();

                _logger.LogDebug($"Deleted {path} from blob storage {_containerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {path} from blob storage {_containerName}");
                throw;
            }
        }

        public async Task<bool?> FileExists(string path)
        {
            bool? exists = null;
            try
            {
                var directory = GetCloudBlobDirectory(GetBlobDirectoryName(path));
                var blob = directory.GetBlockBlobReference(GetBlobFileName(path));

                _logger.LogDebug($"Checking for {path} exists in blob storage {_containerName}");

                exists = await blob.ExistsAsync();

                _logger.LogDebug($"Checked for {path} exists in blob storage {_containerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking {path} exists in blob storage {_containerName}");
                throw;
            }

            return exists;
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
            var container = client.GetContainerReference(_containerName);

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

        private async Task<List<CloudBlob>> GetBlobsHierarchicalListingAsync(CloudBlobDirectory directory, bool recursive)
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
            catch (StorageException ex)
            {
                _logger.LogError(ex, $"Error listing file in blob storage {_containerName}");
                throw;
            }

            return blobs;
        }

        public string GetContainerSasUri(string groupPolicyIdentifier, DateTime startTime, DateTime expiryTime, string ipAddress, SharedAccessBlobPermissions? permissions = null)
        {
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(_containerName);

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = startTime,
                SharedAccessExpiryTime = expiryTime
            };

            if(permissions.HasValue)
            {
                policy.Permissions = permissions.Value;
            }
            else if(string.IsNullOrEmpty(groupPolicyIdentifier))
            {
                throw new Exception("A Sas token cannot be generated when permissions are not specified unless a group policy is used");
            }

            var ipAddressOrRange = !string.IsNullOrEmpty(ipAddress)
                ? new IPAddressOrRange(ipAddress)
                : null;

            var sasContainerToken = container.GetSharedAccessSignature(policy, groupPolicyIdentifier, null, ipAddressOrRange);
            return container.Uri + sasContainerToken;
        }
    }
}
