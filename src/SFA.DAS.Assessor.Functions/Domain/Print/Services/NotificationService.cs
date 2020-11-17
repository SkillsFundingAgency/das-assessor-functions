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

        public async Task Send(int batchNumber, List<Certificate> certificates, string certificatesFileName)
        {
            _logger.Log(LogLevel.Information, $"Inside NotificationService certificatesFileName :: {certificatesFileName}  CertificatesCount :: {certificates.Count()} ");

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);
            
            var personalisationTokens = CreatePersonalisationTokens(certificates, certificatesFileName);

            _logger.Log(LogLevel.Information, "Send Email");

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        private Dictionary<string, string> CreatePersonalisationTokens(List<Certificate> certificates, string certificatesFileName)
        {
            // TODO: The template which is sent need to be re-worked including the parameters below
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
