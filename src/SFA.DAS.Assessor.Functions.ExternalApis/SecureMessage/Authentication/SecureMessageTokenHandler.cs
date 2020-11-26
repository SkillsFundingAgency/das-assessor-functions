using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Authentication
{
    public class SecureMessageTokenHandler : DelegatingHandler
    {
        private readonly ISecureMessageTokenService _tokenService;
        
        public SecureMessageTokenHandler(ISecureMessageTokenService tokenService)
        {
            _tokenService = tokenService;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SetToken(request, await _tokenService.GetToken());
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                SetToken(request, await _tokenService.RefreshToken());
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }

        private void SetToken(HttpRequestMessage request, string token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
