﻿using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class NotificationCommand
    {
        protected readonly IExternalBlobFileTransferClient _externalFileTransferClient;
        protected readonly IInternalBlobFileTransferClient _internalFileTransferClient;

        public NotificationCommand(IExternalBlobFileTransferClient externalFileTransferClient, IInternalBlobFileTransferClient internalFileTransferClient)
        {
            _externalFileTransferClient = externalFileTransferClient;
            _internalFileTransferClient = internalFileTransferClient;
        }

        protected async Task ArchiveFile(string downloadFileContents, string downloadFileName, string downloadDirectoryName, string archiveDirectoryName)
        {
            var archiveFileName = downloadFileName;

            var exists = await _internalFileTransferClient.FileExists($"{archiveDirectoryName}/{downloadFileName}");
            if (exists.GetValueOrDefault(false))
            {
                archiveFileName = archiveFileName.Replace(".json", $"_{DateTime.UtcNow:ddMMyyHHmmss}.json");
            }

            await _internalFileTransferClient.UploadFile(downloadFileContents, $"{archiveDirectoryName}/{archiveFileName}");
            await _externalFileTransferClient.DeleteFile($"{downloadDirectoryName}/{downloadFileName}");
        }
    }
}