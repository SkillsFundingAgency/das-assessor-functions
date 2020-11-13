using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Ilrs;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.Domain.Print;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Assessor.Functions.Domain.Standards;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Authentication;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Configuration;
using SFA.DAS.Assessor.Functions.MockApis.DataCollection;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(SFA.DAS.Assessor.Functions.Startup))]

namespace SFA.DAS.Assessor.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var nLogConfiguration = new NLogConfiguration();

            builder.Services.AddLogging((options) =>
            {
                options.SetMinimumLevel(LogLevel.Trace);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog();
            });

            builder.AddConfiguration((configBuilder) =>
            {
                var configuration = configBuilder
                    .AddAzureTableStorageConfiguration(
                        Environment.GetEnvironmentVariable("ConfigurationStorageConnectionString"),
                        "SFA.DAS.AssessorFunctions",
                        Environment.GetEnvironmentVariable("EnvironmentName"),
                        "1.0")
                    .Build();

                return configuration;
            });

            var config = builder.GetCurrentConfiguration();

            builder.Services.AddOptions();
            
            builder.Services.Configure<AssessorApiAuthentication>(config.GetSection("AssessorApiAuthentication"));
            builder.Services.Configure<DataCollectionApiAuthentication>(config.GetSection("DataCollectionApiAuthentication"));
            builder.Services.Configure<DataCollectionMock>(config.GetSection("DataCollectionMock"));
            builder.Services.Configure<CertificateDetails>(config.GetSection("CertificateDetails"));
            builder.Services.Configure<RefreshIlrsSettings>(config.GetSection("FunctionsSettings:RefreshIlrs"));
            builder.Services.Configure<CertificatePrintFunctionSettings>(config.GetSection("FunctionsSettings:CertificatePrintFunction"));
            builder.Services.Configure<CertificatePrintNotificationFunctionSettings>(config.GetSection("FunctionsSettings:CertificatePrintNotificationFunction"));
            builder.Services.Configure<CertificateDeliveryNotificationFunctionSettings>(config.GetSection("FunctionsSettings:CertificateDeliveryNotificationFunction"));

            builder.Services.AddSingleton<IAssessorServiceTokenService, AssessorServiceTokenService>();
            builder.Services.AddSingleton<IDataCollectionTokenService, DataCollectionTokenService>();
            builder.Services.AddSingleton<ISecureMessageTokenService, SecureMessageTokenService>();

            builder.Services.AddScoped<AssessorTokenHandler>();
            builder.Services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>()
                .AddHttpMessageHandler<AssessorTokenHandler>();

            builder.Services.AddScoped<DataCollectionTokenHandler>();

            var dataCollectionMock = config.GetSection("DataCollectionMock").Get<DataCollectionMock>();
            if (dataCollectionMock.Enabled)
            {
                builder.Services.AddSingleton<IDataCollectionServiceApiClient, DataCollectionMockApiClient>();
            }
            else
            {
                builder.Services.AddHttpClient<IDataCollectionServiceApiClient, DataCollectionServiceApiClient>()
                    .AddHttpMessageHandler<DataCollectionTokenHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        var handler = new HttpClientHandler();
                        if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
                        {
                            // this will disable SSL certificate validation for the LOCAL environment, alternatively obtain a certificate
                            // and install it in the Trusted Root Certificate Authorities for the local machine and then remove this
                            // override to test that a SSL call can be validated correctly by a client certificate
                            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                        }
                        return handler;
                    });
            }

            builder.Services.AddScoped<SecureMessageTokenHandler>();
            builder.Services.AddHttpClient<ISecureMessageServiceApiClient, SecureMessageServiceApiClient>()
                .AddHttpMessageHandler<SecureMessageTokenHandler>();

            builder.Services.AddScoped<IDateTimeHelper, DateTimeHelper>();

            builder.Services.AddScoped<IRefreshIlrsProviderService, RefreshIlrsProviderService>();
            builder.Services.AddScoped<IRefreshIlrsLearnerService, RefreshIlrsLearnerService>();
            
            builder.Services.AddTransient<IRefreshIlrsDequeueProvidersCommand, RefreshIlrsDequeueProvidersCommand>();
            builder.Services.AddTransient<IRefreshIlrsEnqueueProvidersCommand, RefreshIlrsEnqueueProvidersCommand>();

            builder.Services.AddScoped<IBatchService, BatchService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();

            var storageConnectionString = config.GetValue<string>("AzureWebJobsStorage");
            builder.Services.AddTransient<IBlobFileTransferClient>(s =>
                new BlobFileTransferClient(s.GetRequiredService<ILogger<BlobFileTransferClient>>(), storageConnectionString));

            builder.Services.AddTransient<IPrintCreator, PrintingJsonCreator>();
            builder.Services.AddTransient<IPrintProcessCommand, PrintProcessCommand>();
            builder.Services.AddTransient<IDeliveryNotificationCommand, DeliveryNotificationCommand>();
            builder.Services.AddTransient<IPrintNotificationCommand, PrintNotificationCommand>();
            builder.Services.AddTransient<IBlobStorageSamplesCommand, BlobStorageSamplesCommand>();
            builder.Services.AddTransient<IBlobSasTokenGeneratorCommand, BlobSasTokenGeneratorCommand>();

            builder.Services.AddTransient<IStandardCollationImportCommand, StandardCollationImportCommand>();
            builder.Services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();
            
            builder.Services.AddTransient<INotificationService, NotificationService>();
        }
    }
}