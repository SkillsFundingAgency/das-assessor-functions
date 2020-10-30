using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
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
            IOptions<CertificatePrintFunctionSettings> optionsCertificatePrintFunctionSettings,
            IOptions<CertificatePrintNotificationFunctionSettings> optionsCertificatePrintNotificationFunctionSettings,
            IOptions<CertificateDeliveryNotificationFunctionSettings> optionsCertificateDeliveryNotificationFunctionSettings,
            IAssessorServiceApiClient assessorServiceApi)
        {            
            _logger = logger;
            _assessorServiceApi = assessorServiceApi;

            _printRequestDirectory = optionsCertificatePrintFunctionSettings.Value.PrintRequestDirectory;
            _printResponseDirectory = optionsCertificatePrintNotificationFunctionSettings.Value.PrintResponseDirectory;
            _deliveryNotificationDirectory = optionsCertificateDeliveryNotificationFunctionSettings.Value.DeliveryNotificationDirectory;
        }

        public async Task Send(int certificatesCount, string certificatesFileName)
        {
            _logger.LogDebug($"NotificationService::Sending notification of {certificatesCount} certificates in '{certificatesFileName}'");

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);
            
            var personalisationTokens = CreatePersonalisationTokens(certificatesCount, certificatesFileName);

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        private Dictionary<string, string> CreatePersonalisationTokens(int certificatesCount, string certificatesFileName)
        {
            // TODO: The template which is sent need to be re-worked including the parameters below
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
