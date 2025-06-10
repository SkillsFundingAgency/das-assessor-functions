using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class NotificationService : INotificationService
    {        
        private readonly ILogger<NotificationService> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        private readonly string _printRequestDirectory;
        private readonly string _printResponseDirectory;
        private readonly string _deliveryNotificationDirectory;

        public NotificationService(
            ILogger<NotificationService> logger,
            IOptions<PrintRequestOptions> printRequestOptions,
            IOptions<PrintResponseOptions> printResponseOptions,
            IOptions<DeliveryNotificationOptions> deliveryNotificationOptions,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;

            _printRequestDirectory = printRequestOptions.Value.Directory;
            _printResponseDirectory = printResponseOptions.Value.Directory;
            _deliveryNotificationDirectory = deliveryNotificationOptions.Value.Directory;
        }

        public async Task SendPrintRequest(int batchNumber, int certificatesCount, string certificatesFileName)
        {
            _logger.Log(LogLevel.Information, "Sending SendPrintRequest Email for CertificatesFileName: {CertificatesFileName}, CertificatesCount: {CertificatesCount}", certificatesFileName, certificatesCount);

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);
            
            var personalisationTokens = CreatePrintRequestPersonalisationTokens(certificatesCount, certificatesFileName);

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        public async Task SendSasToken(string message)
        {
            _logger.Log(LogLevel.Information, "Sending SendSasToken Email");

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintSasToken);

            var personalisationTokens = CreateSasTokenPersonalisationTokens(message);

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        private static Dictionary<string, string> CreateSasTokenPersonalisationTokens(string secureMessageUri)
        {
            var personalisation = new Dictionary<string, string>
            {
                { "SecretUri", $"{secureMessageUri}" }
            };
            
            return personalisation;
        }

        private Dictionary<string, string> CreatePrintRequestPersonalisationTokens(int certificatesCount, string certificatesFileName)
        {
            // TODO: The template which is sent need to be re-worked as the parameters below should not refer to Sftp
            var personalisation = new Dictionary<string, string>
            {
                {"fileName", $"{certificatesFileName}"},
                {
                    "numberOfCertificatesToBePrinted",
                    $"{certificatesCount}"
                }
            };

            personalisation.Add("SftpPrintRequestDirectory", _printRequestDirectory);
            personalisation.Add("SftpPrintResponseDirectory", _printResponseDirectory);
            personalisation.Add("SftpDeliveryNotificationDirectory", _deliveryNotificationDirectory);

            return personalisation;
        }
    }
}
