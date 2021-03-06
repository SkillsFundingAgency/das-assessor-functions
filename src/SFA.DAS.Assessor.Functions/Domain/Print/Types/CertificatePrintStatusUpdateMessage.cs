﻿using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class CertificatePrintStatusUpdateMessage : CertificatePrintStatusUpdate
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
