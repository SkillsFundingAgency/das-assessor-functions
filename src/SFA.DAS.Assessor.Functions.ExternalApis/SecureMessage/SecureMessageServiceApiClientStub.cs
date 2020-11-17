using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage
{
    public class SecureMessageServiceApiClientStub : ISecureMessageServiceApiClient
    {
        public SecureMessageServiceApiClientStub(
            HttpClient httpClient,
            IOptions<SecureMessageApiAuthentication> options,
            ILogger<SecureMessageServiceApiClient> logger)
        {
        }

        public async Task<CreateMessageResponse> CreateMessage(string message, string ttl)
        {
            var key = Guid.NewGuid().ToString();

            return await Task.FromResult(
                new CreateMessageResponse()
                {
                    Key = key,
                    Links = new CreateMessageResponseLinks()
                    {
                        Api = $"https://tools.apprenticeships.education.gov.uk/api/messages/{key}",
                        Web = $"https://tools.apprenticeships.education.gov.uk/messages/view/{key}"
                    }
                }); ;
        }
    }
}
