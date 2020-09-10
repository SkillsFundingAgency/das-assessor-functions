using Newtonsoft.Json;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Settings
{
    public class ExternalApiDataSync : IExternalApiDataSync
    {
        [JsonRequired] public bool IsEnabled { get; set; }
    }
}