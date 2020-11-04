using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(IAssessorServiceApiClient assessorServiceApiClient, ILogger<CertificateService> logger)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
            _logger = logger;
        }

        public void QueueCertificatePrintStatusUpdateMessages(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates, ICollector<string> storageQueue)
        {
            var messages = certificatePrintStatusUpdates.ChunkBy(10).Select(chunk => new CertificatePrintStatusUpdateMessage()
            {
                CertificatePrintStatusUpdates = chunk
            }).ToList();

            messages.ForEach(p => storageQueue.Add(JsonConvert.SerializeObject(p)));

            _logger.LogInformation($"Queued {messages.Count} messages to update delivery status for {certificatePrintStatusUpdates.Count} certificates");
        }

        public async Task ProcessCertificatesPrintStatusUpdates(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates)
        {
            var model = new CertificatesPrintStatusUpdateRequest()
            {
                CertificatePrintStatusUpdates = certificatePrintStatusUpdates
            };

            var response = await _assessorServiceApiClient.UpdateCertificatesPrintStatus(model);
            if(response.Errors.Count > 0)
            {
            }
        }
    }
}
