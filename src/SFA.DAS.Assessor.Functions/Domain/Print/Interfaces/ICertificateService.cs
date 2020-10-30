using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface ICertificateService
    {
        void QueueCertificatePrintStatusUpdates(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates, ICollector<string> storageQueue);
        Task ProcessCertificatesPrintStatusUpdates(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates);
    }
}
