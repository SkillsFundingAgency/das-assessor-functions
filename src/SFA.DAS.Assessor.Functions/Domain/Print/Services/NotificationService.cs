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
        private readonly SftpSettings _sftpSettings;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public NotificationService(
            ILogger<NotificationService> logger,
            IOptions<SftpSettings> options,
            IAssessorServiceApiClient assessorServiceApi)
        {            
            _logger = logger;
            _sftpSettings = options?.Value;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Send(int batchNumber, List<Certificate> certificates, string certificatesFileName)
        {
            _logger.Log(LogLevel.Information, $"Inside NotificationService certificatesFileName :: {certificatesFileName}  CertificatesCount :: {certificates.Count()} ");

            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);

            _logger.Log(LogLevel.Information, $"emailTemplateSummary.TemplateId :: {emailTemplateSummary.TemplateId}  emailTemplateSummary.TemplateName :: {emailTemplateSummary.TemplateName} ");
            
            var personalisationTokens = CreatePersonalisationTokens(certificates, certificatesFileName);

            _logger.Log(LogLevel.Information, "Send Email");

            await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(string.Empty, emailTemplateSummary, personalisationTokens));
        }

        private Dictionary<string, string> CreatePersonalisationTokens(List<Certificate> certificates, string certificatesFileName)
        {
            _logger.Log(LogLevel.Information, $"certificatesFileName :: {certificatesFileName}  certificates :: {certificates.Count()} ");

            var personalisation = new Dictionary<string, string>
            {
                {"fileName", $"{certificatesFileName}"},
                {
                    "numberOfCertificatesToBePrinted",
                    $"{certificates.Count}"
                }
            };

            if (_sftpSettings.UseJson)
            {
                _logger.Log(LogLevel.Information, $"SftpPrintRequestDirectory :: {_sftpSettings.PrintRequestDirectory}   SftpPrintResponseDirectory ::  {_sftpSettings.PrintResponseDirectory} SftpDeliveryNotificationDirectory :: {_sftpSettings.DeliveryNotificationDirectory} ");

                personalisation.Add("SftpPrintRequestDirectory", _sftpSettings.PrintRequestDirectory);
                personalisation.Add("SftpPrintResponseDirectory", _sftpSettings.PrintResponseDirectory);
                personalisation.Add("SftpDeliveryNotificationDirectory", _sftpSettings.DeliveryNotificationDirectory);
            }
            else
            {
                _logger.Log(LogLevel.Information, $"sftpUploadDirectory :: {_sftpSettings.UploadDirectory}  proofDirectory :: {_sftpSettings.ProofDirectory}  ");
                //these are compatible with the old template and need to be removed as part of tech debt.
                personalisation.Add("sftpUploadDirectory", _sftpSettings.UploadDirectory);
                personalisation.Add("proofDirectory", _sftpSettings.ProofDirectory);
            }

            _logger.Log(LogLevel.Information, $"personalisation :: {personalisation}");

            return personalisation;
        }
    }
}
