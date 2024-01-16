using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication
{
    public class AssessorTokenHandler : DelegatingHandler
    {
        private readonly IAssessorServiceTokenService _tokenService;
        private readonly ILogger<AssessorTokenHandler> _logger;

        public AssessorTokenHandler(IAssessorServiceTokenService tokenService, ILogger<AssessorTokenHandler> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("AssessorTokenHandler - setting token");
            SetToken(request, await _tokenService.GetToken());
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogInformation("AssessorTokenHandler - refreshing token");
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
