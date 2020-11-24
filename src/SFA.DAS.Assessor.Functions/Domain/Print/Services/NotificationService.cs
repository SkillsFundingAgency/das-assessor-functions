using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class NotificationService : INotificationService
    {        
        private readonly ILogger<NotificationService> _logger;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        private string _printRequestDirectory;
        private string _printResponseDirectory;
        private string _deliveryNotificationDirectory;

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

        public async Task SendPrintRequest(int batchNumber, List<Certificate> certificates, string certificatesFileName)
        {
            _logger.Log(LogLevel.Information, $"SendPrintRequest CertificatesFileName({certificatesFileName}),  CertificatesCount({certificates.Count()}) Email");

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);
            
            var personalisationTokens = CreatePrintRequestPersonalisationTokens(certificates, certificatesFileName);

            _logger.Log(LogLevel.Information, "NotificationService::SendPrintRequest Email");

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        public async Task SendSasToken(string secureMessageUri)
        {
            _logger.Log(LogLevel.Information, "SendSasToken Email");

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintSasToken);

            var personalisationTokens = CreateSasTokenPersonalisationTokens(secureMessageUri);

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        private Dictionary<string, string> CreateSasTokenPersonalisationTokens(string secureMessageUri)
        {
            var personalisation = new Dictionary<string, string>
            {
                { "SecretUri", $"{secureMessageUri}" }
            };
            
            return personalisation;
        }

        private Dictionary<string, string> CreatePrintRequestPersonalisationTokens(List<Certificate> certificates, string certificatesFileName)
        {
            // TODO: The template which is sent need to be re-worked as the parameters below should not refer to Sftp
            var personalisation = new Dictionary<string, string>
            {
                {"fileName", $"{certificatesFileName}"},
                {
                    "numberOfCertificatesToBePrinted",
                    $"{certificates.Count}"
                }
            };

            personalisation.Add("SftpPrintRequestDirectory", _printRequestDirectory);
            personalisation.Add("SftpPrintResponseDirectory", _printResponseDirectory);
            personalisation.Add("SftpDeliveryNotificationDirectory", _deliveryNotificationDirectory);

            return personalisation;
        }
    }
}
