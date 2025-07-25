﻿using Azure.Storage.Sas;

namespace SFA.DAS.Assessor.Functions.Domain.FileTransfer
{
    public interface IBlobFileTransferClient
    {
        string ContainerName { get; }
        Task<List<string>> GetFileNames(string directory, bool recursive);
        Task<List<string>> GetFileNames(string directory, string pattern, bool recursive);
        Task UploadFile(string fileContents, string path);
        Task<bool?> FileExists(string path);
        Task<string> DownloadFile(string path);
        Task DeleteFile(string path);
        string GetContainerSasUri(string groupPolicyIdentifier, DateTime startTime, DateTime expiryTime, string ipAddress, BlobSasPermissions? permissions = null);
    }
}
