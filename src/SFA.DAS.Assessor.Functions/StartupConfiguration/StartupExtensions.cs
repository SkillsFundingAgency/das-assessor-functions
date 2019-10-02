using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ApiClient;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SFA.DAS.Assessor.Functions.StartupConfiguration
{
    public static class StartupExtensions
    {
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, AssessorApiAuthentication assessorApiAuthenticationOptions, IConfiguration configuration)
        {
            var assessorBaseAddress = assessorApiAuthenticationOptions?.ApiBaseAddress;

            var assessorHttpClient = new AssessorHttpClient
            {
                BaseAddress = new Uri(assessorBaseAddress)
            };

            var tokenService = new AssessorTokenService(assessorApiAuthenticationOptions, configuration);
            var token = tokenService.GetToken();

            assessorHttpClient.DefaultRequestHeaders.Accept.Clear();
            assessorHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            assessorHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            services.AddSingleton(assessorHttpClient);

            return services;
        }

        public static IServiceCollection ConfigureDependencies(this IServiceCollection services)
        {
            services.AddTransient<IAssessorServiceApiClient, AssessorServiceApiClient>();

            return services;
        }
    }
}
