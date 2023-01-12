using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Types;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage
{
    public class SecureMessageServiceApiClient : ApiClientBase, ISecureMessageServiceApiClient
    {
        public SecureMessageServiceApiClient(
            HttpClient httpClient,
            IOptions<SecureMessageApiAuthentication> options,
            ILogger<SecureMessageServiceApiClient> logger)
            : base(httpClient, new Uri(options?.Value?.ApiBaseAddress), logger)
        {
        }

        public async Task<CreateMessageResponse> CreateMessage(string message, string ttl)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/messages"))
            {
                return await PostPutRequestWithResponse<CreateMessageRequest, CreateMessageResponse>(request, new CreateMessageRequest { Message = message, Ttl = ttl });
            }
        }
    }
}
