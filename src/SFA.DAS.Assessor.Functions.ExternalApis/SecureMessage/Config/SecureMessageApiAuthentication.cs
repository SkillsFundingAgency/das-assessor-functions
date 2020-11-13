﻿using SFA.DAS.Assessor.Functions.ExternalApis.Config;

namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config
{
    public class SecureMessageApiAuthentication
    {
        public OAuth OAuth { get; set; }
        public string ResourceId { get; set; }
        public string ApiBaseAddress { get; set; }
    }
}
