using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Constants;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationsApi _notificationsApi;
        private readonly ILogger<NotificationService> _logger;
        private readonly SftpSettings _sftpSettings;
        private readonly IAssessorServiceApiClient _assessorServiceApi;

        public NotificationService(INotificationsApi notificationsApi,
            ILogger<NotificationService> logger,
            IOptions<SftpSettings> options,
            IAssessorServiceApiClient assessorServiceApi)
        {
            _notificationsApi = notificationsApi;
            _logger = logger;
            _sftpSettings = options?.Value;
            _assessorServiceApi = assessorServiceApi;
        }

        public async Task Send(int batchNumber, List<CertificateToBePrintedSummary> certificateResponses, string certificatesFileName)
        {
            var emailTemplate = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);

            var personalisation = CreatePersonalisationTokens(certificateResponses, certificatesFileName);

            _logger.Log(LogLevel.Information, "Send Email");

            var recipients = emailTemplate.Recipients.Split(';').Select(x => x.Trim());
            foreach (var recipient in recipients)
            {
                var email = new Email
                {
                    RecipientsAddress = recipient,
                    TemplateId = emailTemplate.TemplateId,
                    ReplyToAddress = "jcoxhead@hotmail.com",
                    Subject = "Test Subject",
                    SystemId = "PrintAssessorCoverLetters",
                    Tokens = personalisation
                };

                await _notificationsApi.SendEmail(email);
            }
        }

        private Dictionary<string, string> CreatePersonalisationTokens(List<CertificateToBePrintedSummary> certificateResponses, string certificatesFileName)
        {
            var personalisation = new Dictionary<string, string>
            {
                {"fileName", $"{certificatesFileName}"},
                {
                    "numberOfCertificatesToBePrinted",
                    $"{certificateResponses.Count}"
                },
                {"numberOfCoverLetters", ""},
                {"sftpUploadDirectory", $"{_sftpSettings.UploadDirectory}"},
                {"proofDirectory", $"{_sftpSettings.ProofDirectory}"}
            };
            return personalisation;
        }
    }
}
