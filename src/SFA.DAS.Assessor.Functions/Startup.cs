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
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Configuration;
using SFA.DAS.Assessor.Functions.Infrastructure.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
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

            builder.Services.Configure<AssessorApiAuthentication>(config.GetSection(nameof(AssessorApiAuthentication)));
            builder.Services.Configure<DataCollectionApiAuthentication>(config.GetSection(nameof(DataCollectionApiAuthentication)));
            builder.Services.Configure<SecureMessageApiAuthentication>(config.GetSection(nameof(SecureMessageApiAuthentication)));
            builder.Services.Configure<DataCollectionMock>(config.GetSection(nameof(DataCollectionMock)));

            var functionsOptions = nameof(FunctionsOptions);
            builder.Services.Configure<RefreshIlrsOptions>(config.GetSection($"{functionsOptions}:{nameof(RefreshIlrsOptions)}"));

            var printCertificatesOptions = $"{functionsOptions}:{nameof(PrintCertificatesOptions)}";
            builder.Services.Configure<PrintRequestOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(PrintRequestOptions)}"));
            builder.Services.Configure<CertificateDetails>(config.GetSection($"{printCertificatesOptions}:{nameof(PrintRequestOptions)}:{nameof(CertificateDetails)}"));
            builder.Services.Configure<PrintResponseOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(PrintResponseOptions)}"));
            builder.Services.Configure<DeliveryNotificationOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(DeliveryNotificationOptions)}"));
            builder.Services.Configure<BlobSasTokenGeneratorOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(BlobSasTokenGeneratorOptions)}"));

            builder.Services.AddSingleton<IAssessorServiceTokenService, AssessorServiceTokenService>();
            builder.Services.AddSingleton<IDataCollectionTokenService, DataCollectionTokenService>();

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

            builder.Services.AddScoped<IDateTimeHelper, DateTimeHelper>();

            builder.Services.AddScoped<IRefreshIlrsProviderService, RefreshIlrsProviderService>();
            builder.Services.AddScoped<IRefreshIlrsLearnerService, RefreshIlrsLearnerService>();
            
            builder.Services.AddTransient<IRefreshIlrsDequeueProvidersCommand, RefreshIlrsDequeueProvidersCommand>();
            builder.Services.AddTransient<IRefreshIlrsEnqueueProvidersCommand, RefreshIlrsEnqueueProvidersCommand>();

            builder.Services.AddScoped<IBatchService, BatchService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();

            var storageConnectionString = config.GetValue<string>("AzureWebJobsStorage");
            var optionsCertificateFunctions = config.GetSection(functionsOptions).GetSection(nameof(printCertificatesOptions)).Get<PrintCertificatesOptions>();

            builder.Services.AddTransient(s => new BlobFileTransferClient(
                s.GetRequiredService<ILogger<BlobFileTransferClient>>(),
                storageConnectionString,
                optionsCertificateFunctions.ExternalBlobContainer) as IExternalBlobFileTransferClient);

            builder.Services.AddTransient(s => new BlobFileTransferClient(
                s.GetRequiredService<ILogger<BlobFileTransferClient>>(),
                storageConnectionString,
                optionsCertificateFunctions.InternalBlobContainer) as IInternalBlobFileTransferClient);

            builder.Services.AddTransient<IPrintCreator, PrintingJsonCreator>();
            builder.Services.AddTransient<IPrintRequestCommand, PrintRequestCommand>();
            builder.Services.AddTransient<IDeliveryNotificationCommand, DeliveryNotificationCommand>();
            builder.Services.AddTransient<IPrintResponseCommand, PrintResponseCommand>();
            builder.Services.AddTransient<IPrintStatusUpdateCommand, PrintStatusUpdateCommand>();
            builder.Services.AddTransient<IBlobStorageSamplesCommand, BlobStorageSamplesCommand>();
            
            builder.Services.AddTransient<IStandardCollationImportCommand, StandardCollationImportCommand>();
            builder.Services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();
            
            builder.Services.AddTransient<INotificationService, NotificationService>();
        }
    }
}