using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
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

        
        public async Task Send(int batchNumber, List<CertificateResponse> certificateResponses, string certificatesFileName)
        {
            var emailTemplateSummary = await _assessorServiceApi.GetEmailTemplate(EMailTemplateNames.PrintAssessorCoverLetters);

            var recipients = emailTemplateSummary.Recipients.Split(';').Select(x => x.Trim());

            var personalisationTokens = CreatePersonalisationTokens(certificateResponses, certificatesFileName);

            _logger.Log(LogLevel.Information, "Send Email");

            foreach (var recipient in recipients)
            {
                await _assessorServiceApi.SendEmailWithTemplate(new SendEmailRequest(recipient, emailTemplateSummary, personalisationTokens));
            }

        }

        private Dictionary<string, string> CreatePersonalisationTokens(List<CertificateResponse> certificateResponses, string certificatesFileName)
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
