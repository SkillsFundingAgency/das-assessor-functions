using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types.Notifications;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.Domain.Print.Exceptions;
using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print
{
    public class DeliveryNotificationCommand : NotificationCommand, IDeliveryNotificationCommand
    {
        // DeliveryNotifications-ddMMyyHHmm.json
        private static readonly string DateTimePatternFormat = "ddMMyyHHmm";
        private static readonly string DateTimePatternRegEx = "[0-9]{10}";
        private static readonly string FilePatternRegEx = $@"^[Dd][Ee][Ll][Ii][Vv][Ee][Rr][Yy][Nn][Oo][Tt][Ii][Ff][Ii][Cc][Aa][Tt][Ii][Oo][Nn][Ss]-{DateTimePatternRegEx}.json";

        private readonly ILogger<DeliveryNotificationCommand> _logger;
        private readonly ICertificateService _certificateService;
        private readonly DeliveryNotificationOptions _options;

        public DeliveryNotificationCommand(
            ILogger<DeliveryNotificationCommand> logger,
            ICertificateService certificateService,
            IExternalBlobFileTransferClient externalFileTransferClient,
            IInternalBlobFileTransferClient internalFileTransferClient,
            IOptions<DeliveryNotificationOptions> options)
            : base(externalFileTransferClient, internalFileTransferClient)
        {
            _logger = logger;
            _certificateService = certificateService;
            _options = options?.Value;
        }

        public async Task<List<CertificatePrintStatusUpdateMessage>> Execute()
        {
            var printStatusUpdateMessages = new List<CertificatePrintStatusUpdateMessage>();

            _logger.Log(LogLevel.Information, "PrintDeliveryNotificationCommand - Started");

            var fileNames = await _externalFileTransferClient.GetFileNames(_options.Directory, FilePatternRegEx, false);

            if (!fileNames.Any())
            {
                _logger.LogInformation("No certificate delivery notifications from the printer are available to process");
                return null;
            }

            var sortedFileNames = fileNames.ToList().SortByDateTimePattern(DateTimePatternRegEx, DateTimePatternFormat);
            foreach (var fileName in sortedFileNames)
            {
                try
                {
                    var fileContents = await _externalFileTransferClient.DownloadFile($"{_options.Directory}/{fileName}");
                    var fileInfo = new PrintFileInfo(fileContents, fileName);

                    try
                    {
                        var messages = ProcessDeliveryNotifications(fileInfo);

                        if (fileInfo.ValidationMessages.Count > 0)
                        {
                            _logger.LogError($"The delivery notification file [{fileInfo.FileName}] contained invalid entries, an error file has been created");
                            await CreateErrorFile(fileInfo, _options.Directory, _options.ErrorDirectory);
                        }

                        await ArchiveFile(fileContents, fileName, _options.Directory, _options.ArchiveDirectory);
                        printStatusUpdateMessages.AddRange(messages);
                    }
                    catch(FileFormatValidationException ex)
                    {
                        fileInfo.ValidationMessages.Add(ex.Message);
                        await CreateErrorFile(fileInfo, _options.Directory, _options.ErrorDirectory);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Could not process delivery notification file [{fileName}]");
                }
            }
            
            return printStatusUpdateMessages;
        }

        private List<CertificatePrintStatusUpdateMessage> ProcessDeliveryNotifications(PrintFileInfo fileInfo)
        {
            var receipt = JsonConvert.DeserializeObject<DeliveryReceipt>(fileInfo.FileContent);

            if (receipt?.DeliveryNotifications == null)
            {
                fileInfo.InvalidFileContent = fileInfo.FileContent;
                throw new FileFormatValidationException($"Could not process delivery notification file [{fileInfo.FileName}] due to invalid file format");
            }

            return receipt.DeliveryNotifications.Select(n => new CertificatePrintStatusUpdateMessage
            {
                CertificateReference = n.CertificateNumber,
                BatchNumber = n.BatchID,
                Status = n.Status,
                StatusAt = n.StatusChangeDate,
                ReasonForChange = n.Reason
            }).ToList();
        }
    }
}
