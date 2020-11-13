using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage
{
    public interface ISecureMessageServiceApiClient
    {
        Task<CreateMessageResponse> CreateMessage(string message, string ttl);
    }

    public class TtlConstants
    {
        public const string Hour = "Hour";
        public const string Day = "Day";
    }
}
