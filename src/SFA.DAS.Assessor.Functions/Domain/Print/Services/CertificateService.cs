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

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IAssessorServiceApiClient _assessorServiceApiClient;

        public CertificateService(IAssessorServiceApiClient assessorServiceApiClient)
        {
            _assessorServiceApiClient = assessorServiceApiClient;
        }

        public void QueueCertificatePrintStatusUpdates(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates, ICollector<string> storageQueue)
        {
            foreach (var chunk in certificatePrintStatusUpdates.ChunkBy(10))
            {
                var message = new CertificatePrintStatusUpdateMessage()
                {
                    CertificatePrintStatusUpdates = chunk
                };

                storageQueue.Add(JsonConvert.SerializeObject(message));
            }
        }

        public async Task ProcessCertificatesPrintStatusUpdates(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates)
        {
            await _assessorServiceApiClient.UpdatePrintStatus(certificatePrintStatusUpdates);
        }
    }
}
