using System;
using System.Globalization;

namespace SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication
{
    public class DataCollectionApiAuthentication
    {
        public string Instance => "https://login.microsoftonline.com/{0}";
        public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, TenantId);    
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string Version { get; set; }
        public string ApiBaseAddress { get; set; }
    }
}