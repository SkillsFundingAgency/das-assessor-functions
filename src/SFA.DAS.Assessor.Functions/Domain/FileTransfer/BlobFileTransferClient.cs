﻿using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace SFA.DAS.Assessor.Functions.Domain.FileTransfer
{
    public class BlobFileTransferClient : IExternalBlobFileTransferClient, IInternalBlobFileTransferClient, IOfqualDownloadsBlobFileTransferClient
    {
        private readonly ILogger<BlobFileTransferClient> _logger;
        private BlobContainerClient _blobContainerClient;

        public BlobFileTransferClient(ILogger<BlobFileTransferClient> logger, string connectionString, string containerName)
        {
            _logger = logger;

            var blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            try
            {
                _blobContainerClient.CreateIfNotExists();
                _logger.LogInformation($"Blob container '{containerName}' ensured to exist.");
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                _logger.LogWarning($"Blob container '{containerName}' already exists. Details: {ex.Message}");
                // Optionally, you can choose to ignore or handle this scenario differently.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while creating/accessing blob container '{containerName}'.");
                throw; // Re-throw to ensure the error is propagated.
            }
        }

        public string ContainerName => _blobContainerClient.Name;

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
                var blobs = await GetBlobsHierarchicalListingAsync(directory, recursive);
                fileNames.AddRange(blobs.ConvertAll(p => GetBlobFileName(p.Name)));
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
                string directoryName = GetBlobDirectoryName(path);
                string blobName = GetBlobFileName(path);
                string fullBlobName = string.IsNullOrEmpty(directoryName) ? blobName : $"{directoryName}/{blobName}";

                _logger.LogDebug($"Uploading {path} to blob storage {ContainerName}");

                BlobClient blobClient = _blobContainerClient.GetBlobClient(fullBlobName);
                byte[] array = Encoding.UTF8.GetBytes(fileContents);

                using (var stream = new MemoryStream(array))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                _logger.LogDebug($"Uploaded {path} to blob storage {ContainerName}");
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
                string directoryName = GetBlobDirectoryName(path);
                string blobName = GetBlobFileName(path);
                string fullBlobName = string.IsNullOrEmpty(directoryName) ? blobName : $"{directoryName}/{blobName}";

                _logger.LogDebug($"Downloading {path} from blob storage {_blobContainerClient.Name}");

                BlobClient blobClient = _blobContainerClient.GetBlobClient(fullBlobName);

                using (var memoryStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(memoryStream);
                    fileContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                }

                _logger.LogDebug($"Downloaded {path} from blob storage {_blobContainerClient.Name}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading {path} from blob storage {_blobContainerClient.Name}");
                throw;
            }

            return fileContent;
        }

        public async Task DeleteFile(string path)
        {
            try
            {
                string directoryName = GetBlobDirectoryName(path);
                string blobName = GetBlobFileName(path);
                string fullBlobName = string.IsNullOrEmpty(directoryName) ? blobName : $"{directoryName}/{blobName}";

                _logger.LogDebug($"Deleting {path} from blob storage {_blobContainerClient.Name}");

                BlobClient blobClient = _blobContainerClient.GetBlobClient(fullBlobName);
                await blobClient.DeleteIfExistsAsync();

                _logger.LogDebug($"Deleted {path} from blob storage {_blobContainerClient.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {path} from blob storage {_blobContainerClient.Name}");
                throw;
            }
        }

        public async Task<bool?> FileExists(string path)
        {
            bool? exists = null;
            try
            {
                string directoryName = GetBlobDirectoryName(path);
                string blobName = GetBlobFileName(path);
                string fullBlobName = string.IsNullOrEmpty(directoryName) ? blobName : $"{directoryName}/{blobName}";

                BlobClient blobClient = _blobContainerClient.GetBlobClient(fullBlobName);
                exists = await blobClient.ExistsAsync();

                _logger.LogDebug($"Checked if {path} exists in blob storage {_blobContainerClient.Name}: {exists}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking {path} exists in blob storage {_blobContainerClient.Name}");
                throw;
            }

            return exists;
        }

        private static string GetBlobFileName(string path)
        {
            return Path.GetFileName(path);
        }

        private static string GetBlobDirectoryName(string path)
        {
            var directoryName = Path.GetDirectoryName(path);

            directoryName = !string.IsNullOrEmpty(directoryName)
                ? directoryName.Replace('\\', '/').TrimStart('/')
                : path;

            return directoryName.EndsWith('/')
                ? directoryName
                : directoryName += '/';
        }

        private async Task<List<BlobItem>> GetBlobsHierarchicalListingAsync(string prefix, bool recursive)
        {
            var blobs = new List<BlobItem>();

            try
            {
                await foreach (BlobHierarchyItem blobItem in _blobContainerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/"))
                {
                    if (blobItem.IsPrefix && recursive)
                    {
                        blobs.AddRange(await GetBlobsHierarchicalListingAsync(blobItem.Prefix, recursive));
                    }
                    else if (blobItem.IsBlob)
                    {
                        blobs.Add(blobItem.Blob);
                    }
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, $"Error listing file in blob storage {_blobContainerClient.Name} with prefix '{prefix}'");
                throw;
            }

            return blobs;
        }

        public string GetContainerSasUri(string groupPolicyIdentifier, DateTime startTime, DateTime expiryTime, string ipAddress, BlobSasPermissions? permissions = null)
        {
            if (permissions == null && string.IsNullOrEmpty(groupPolicyIdentifier))
            {
                throw new Exception("A Sas token cannot be generated when permissions are not specified unless a group policy is used");
            }
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _blobContainerClient.Name,
                Resource = "c", // 'c' for container
                StartsOn = startTime,
                ExpiresOn = expiryTime
            };

            if (permissions.HasValue)
            {
                sasBuilder.SetPermissions(permissions.Value);
            }

            if (!string.IsNullOrEmpty(groupPolicyIdentifier))
            {
                sasBuilder.Identifier = groupPolicyIdentifier;
            }

            if (!string.IsNullOrEmpty(ipAddress))
            {
                sasBuilder.IPRange = SasIPRange.Parse(ipAddress);
            }

            Uri sasUri = _blobContainerClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }
    }
}
