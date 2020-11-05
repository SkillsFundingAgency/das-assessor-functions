﻿using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface ICertificateService
    {
        void QueueCertificatePrintStatusUpdateMessages(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates, ICollector<string> storageQueue, int maxCertificatesToUpdate);
        Task ProcessCertificatesPrintStatusUpdates(List<CertificatePrintStatusUpdate> certificatePrintStatusUpdates);
    }
}
