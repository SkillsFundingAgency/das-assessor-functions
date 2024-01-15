using SFA.DAS.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Config
{
    public class ManagedIdentityClientConfiguration : IManagedIdentityClientConfiguration
    {
        public string IdentifierUri { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}
