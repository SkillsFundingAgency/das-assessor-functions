using Microsoft.Azure.Cosmos.Table;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public class ConfigurationItem : TableEntity
    {
        public string Data { get; set; }
    }
}