﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Interfaces
{
    public interface IManagedIdentityClientConfiguration
    {
        string IdentifierUri { get; set; }
        string ApiBaseUrl { get; set; }
    }
}
