using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using System;
using System.Net.Http;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationService(this IServiceCollection services)
        {
            services.AddTransient<INotificationsApiClientConfiguration>(c =>
            {
                var config = c.GetService<IOptions<NotificationsApiClientConfiguration>>().Value;

                return new NotificationsApiClientConfiguration
                {
                    ApiBaseUrl = config.ApiBaseUrl,
                    #pragma warning disable 618
                    ClientToken = config.ClientToken,
                    #pragma warning restore 618
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret,
                    IdentifierUri = config.IdentifierUri,
                    Tenant = config.Tenant
                };
            });

            services.AddTransient<INotificationsApi>(c => new NotificationsApi(GetHttpClient(c), c.GetRequiredService<INotificationsApiClientConfiguration>()));
            services.AddTransient<INotificationService, NotificationService>();

            return services;
        }

        private static HttpClient GetHttpClient(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<INotificationsApiClientConfiguration>();
            
            var httpClient = string.IsNullOrWhiteSpace(config.ClientId)
                ? new HttpClientBuilder().WithBearerAuthorisationHeader(new JwtBearerTokenGenerator(config)).Build()
                :  new HttpClientBuilder().WithBearerAuthorisationHeader(new AzureActiveDirectoryBearerTokenGenerator(config)).Build();

            return httpClient;
        }
    }
}
