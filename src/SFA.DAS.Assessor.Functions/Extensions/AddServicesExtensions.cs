using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.ExternalApiDataSync;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using SFA.DAS.Assessor.Functions.Infrastructure.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.OfqualImport;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.Learners;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.Providers;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage;
using SFA.DAS.Assessor.Functions.MockApis.DataCollection;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync;
using SFA.DAS.Assessor.Functions.Domain.Ilrs;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Learners;
using SFA.DAS.Assessor.Functions.Domain.Ofs;
using SFA.DAS.Assessor.Functions.Domain.Print;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Standards;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.FileTransfer;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions;
using SFA.DAS.AssessorService.Functions.Data;
using SFA.DAS.Assessor.Functions.Infrastructure.Queues;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.RequestApprenticeTraining.Functions.Extensions
{
    public static class AddServicesExtensions
    {
        public static IServiceCollection AddAllServices(
        this IServiceCollection services,
        IConfiguration config)
        {
            return services
                .AddOptionsServices(config)
                .AddHttpClientsServices(config)
                .AddDomainServices(config)
                .AddCommandsServices(config)
                .AddInfrastructureServices(config);
        }

        public static IServiceCollection AddOptionsServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddOptions();

            services.Configure<AssessorManagedIdentityClientConfiguration>(
                config.GetSection(nameof(AssessorManagedIdentityClientConfiguration)));
            services.Configure<DataCollectionApiAuthentication>(
                config.GetSection(nameof(DataCollectionApiAuthentication)));
            services.Configure<OfsRegisterApiAuthentication>(
                config.GetSection(nameof(OfsRegisterApiAuthentication)));
            services.Configure<SecureMessageApiAuthentication>(
                config.GetSection(nameof(SecureMessageApiAuthentication)));
            services.Configure<DataCollectionMock>(
                config.GetSection(nameof(DataCollectionMock)));

            var functionsOptions = nameof(FunctionsOptions);

            services.Configure<FunctionsOptions>(
                config.GetSection(functionsOptions));

            services.Configure<RebuildExternalApiSandboxOptions>(
                config.GetSection($"{functionsOptions}:{nameof(RebuildExternalApiSandboxOptions)}"));
            var section1 = config.GetSection($"{functionsOptions}:{nameof(RebuildExternalApiSandboxOptions)}");


            services.Configure<RefreshIlrsOptions>(
                config.GetSection($"{functionsOptions}:{nameof(RefreshIlrsOptions)}"));

            var databaseMaintenanceOptions = $"{functionsOptions}:{nameof(DatabaseMaintenanceOptions)}";
            services.Configure<DatabaseMaintenanceOptions>(
                config.GetSection(databaseMaintenanceOptions));

            var ofqualImportOptions = $"{functionsOptions}:{nameof(OfqualImportOptions)}";
            services.Configure<OfqualImportOptions>(
                config.GetSection(ofqualImportOptions));

            var printCertificatesOptions = $"{functionsOptions}:{nameof(PrintCertificatesOptions)}";

            services.Configure<PrintCertificatesOptions>(
                config.GetSection($"{printCertificatesOptions}"));

            services.Configure<PrintRequestOptions>(
                config.GetSection($"{printCertificatesOptions}:{nameof(PrintRequestOptions)}"));

            services.Configure<CertificateDetails>(
                config.GetSection($"{printCertificatesOptions}:{nameof(PrintRequestOptions)}:{nameof(CertificateDetails)}"));

            services.Configure<PrintResponseOptions>(
                config.GetSection($"{printCertificatesOptions}:{nameof(PrintResponseOptions)}"));

            services.Configure<DeliveryNotificationOptions>(
                config.GetSection($"{printCertificatesOptions}:{nameof(DeliveryNotificationOptions)}"));

            services.Configure<BlobSasTokenGeneratorOptions>(
                config.GetSection($"{printCertificatesOptions}:{nameof(BlobSasTokenGeneratorOptions)}"));

            var importLearnerOptions = $"{functionsOptions}:{nameof(ImportLearnersOptions)}";
            services.Configure<ImportLearnersOptions>(
                config.GetSection(importLearnerOptions));

            var refreshProvidersOptions = $"{functionsOptions}:{nameof(RefreshProvidersOptions)}";
            services.Configure<RefreshProvidersOptions>(
                config.GetSection(refreshProvidersOptions));

            services.Configure<OuterApi>(
                config.GetSection(nameof(OuterApi)));

            return services;
        }

        public static IServiceCollection AddHttpClientsServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddSingleton<IAssessorServiceTokenService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AssessorServiceTokenService>>();
                var assessorOptions = sp.GetRequiredService<IOptions<AssessorManagedIdentityClientConfiguration>>().Value;

                return new AssessorServiceTokenService(
                    new ManagedIdentityTokenGenerator(assessorOptions),
                    logger);
            });

            services.AddSingleton<IDataCollectionTokenService, DataCollectionTokenService>();
            services.AddSingleton<ISecureMessageTokenService, SecureMessageTokenService>();

            services.AddScoped<AssessorTokenHandler>();
            services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>()
                    .AddHttpMessageHandler<AssessorTokenHandler>();

            services.AddScoped<DataCollectionTokenHandler>();

            var dataCollectionMock = config
                .GetSection(nameof(DataCollectionMock))
                .Get<DataCollectionMock>();

            if (dataCollectionMock != null && dataCollectionMock.Enabled)
            {
                services.AddSingleton<IDataCollectionServiceApiClient, DataCollectionMockApiClient>();
            }
            else
            {
                services.AddHttpClient<IDataCollectionServiceApiClient, DataCollectionServiceApiClient>()
                    .AddHttpMessageHandler<DataCollectionTokenHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        var handler = new HttpClientHandler();
                        var envName = Environment.GetEnvironmentVariable("EnvironmentName");
                        if (string.Equals("LOCAL", envName, StringComparison.OrdinalIgnoreCase))
                        {
                            // this will disable SSL certificate validation for the LOCAL environment, alternatively obtain a certificate
                            // and install it in the Trusted Root Certificate Authorities for the local machine and then remove this
                            // override to test that a SSL call can be validated correctly by a client certificate

                            handler.ServerCertificateCustomValidationCallback =
                                (msg, cert, chain, errors) => true;
                        }
                        return handler;
                    });
            }

            services.AddScoped<SecureMessageTokenHandler>();

            var environmentName = Environment.GetEnvironmentVariable("EnvironmentName");
            if (string.Equals("LOCAL", environmentName, StringComparison.OrdinalIgnoreCase))
            {
                services.AddHttpClient<ISecureMessageServiceApiClient, SecureMessageServiceApiClientStub>();
            }
            else
            {
                services.AddHttpClient<ISecureMessageServiceApiClient, SecureMessageServiceApiClient>()
                        .AddHttpMessageHandler<SecureMessageTokenHandler>();
            }

            services.AddHttpClient<IOuterApiClient, OuterApiClient>();
            services.AddHttpClient<IOfsRegisterApiClient, OfsRegisterApiClient>();

            services.AddHttpClient(
                OfqualDataType.Organisations.ToString(),
                (sp, httpClient) =>
                {
                    var opts = sp.GetRequiredService<IOptions<OfqualImportOptions>>().Value;
                    httpClient.BaseAddress = new Uri(opts.OrganisationsDataUrl);
                });

            services.AddHttpClient(
                OfqualDataType.Qualifications.ToString(),
                (sp, httpClient) =>
                {
                    var opts = sp.GetRequiredService<IOptions<OfqualImportOptions>>().Value;
                    httpClient.BaseAddress = new Uri(opts.QualificationsDataUrl);
                });

            return services;
        }

        public static IServiceCollection AddDomainServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddScoped<IDateTimeHelper, DateTimeHelper>();

            services.AddScoped<IRefreshIlrsAccessorSettingService, RefreshIlrsAccessorSettingService>();
            services.AddScoped<IRefreshIlrsAcademicYearService, RefreshIlrsAcademicYearService>();
            services.AddScoped<IRefreshIlrsProviderService, RefreshIlrsProviderService>();
            services.AddScoped<IRefreshIlrsLearnerService, RefreshIlrsLearnerService>();

            services.AddScoped<IBatchService, BatchService>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }

        public static IServiceCollection AddCommandsServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddTransient<IDatabaseMaintenanceCommand, DatabaseMaintenanceCommand>();

            services.AddTransient<IPrintRequestCommand, PrintRequestCommand>();
            services.AddTransient<IPrintResponseCommand, PrintResponseCommand>();
            services.AddTransient<IDeliveryNotificationCommand, DeliveryNotificationCommand>();
            services.AddTransient<IPrintStatusUpdateCommand, PrintStatusUpdateCommand>();
            services.AddTransient<IBlobStorageSamplesCommand, BlobStorageSamplesCommand>();
            services.AddTransient<IBlobSasTokenGeneratorCommand, BlobSasTokenGeneratorCommand>();
            services.AddTransient<IRebuildExternalApiSandboxCommand, RebuildExternalApiSandboxCommand>();

            services.AddTransient<IRefreshIlrsDequeueProvidersCommand, RefreshIlrsDequeueProvidersCommand>();
            services.AddTransient<IRefreshIlrsEnqueueProvidersCommand, RefreshIlrsEnqueueProvidersCommand>();

            services.AddTransient<IStandardImportCommand, StandardImportCommand>();
            services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();

            services.AddTransient<IImportLearnersCommand, ImportLearnersCommand>();
            services.AddTransient<IRefreshProvidersCommand, RefreshProvidersCommand>();

            services.AddTransient<IOfsImportCommand, OfsImportCommand>();

            services.AddTransient<IEnqueueLearnerInfoCommand, EnqueueLearnerInfoCommand>();
            services.AddTransient<IDequeueLearnerInfoCommand, DequeueLearnerInfoCommand>();
            services.AddTransient<IEnqueueApprovalLearnerInfoBatchCommand, EnqueueApprovalLearnerInfoBatchCommand>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            var storageConnectionString = config.GetValue<string>("AzureWebJobsStorage");

            services.AddTransient<IExternalBlobFileTransferClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BlobFileTransferClient>>();

                var printCertificatesOptions = sp
                   .GetRequiredService<IOptions<PrintCertificatesOptions>>()
                   .Value;

                return new BlobFileTransferClient(logger,
                    storageConnectionString,
                    printCertificatesOptions.ExternalBlobContainer);
            });

            services.AddTransient<IInternalBlobFileTransferClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BlobFileTransferClient>>();
                var printCertificatesOptions = sp
                   .GetRequiredService<IOptions<PrintCertificatesOptions>>()
                   .Value;

                return new BlobFileTransferClient(logger,
                    storageConnectionString,
                    printCertificatesOptions.InternalBlobContainer);
            });

            services.AddTransient<IOfqualDownloadsBlobFileTransferClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BlobFileTransferClient>>();

                var ofqualImportOptions = sp
                    .GetRequiredService<IOptions<OfqualImportOptions>>()
                    .Value;

                return new BlobFileTransferClient(logger,
                    storageConnectionString,
                    ofqualImportOptions.DownloadBlobContainer);
            });

            services.AddTransient<IPrintCreator, PrintingJsonCreator>();

            var optionsFunctions = services.BuildServiceProvider().GetService<IOptions<FunctionsOptions>>();
            services.AddDatabaseRegistration(optionsFunctions);

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IAssessorServiceRepository, AssessorServiceRepository>();

            services.AddSingleton<IQueueClientFactory>(sp => new QueueClientFactory(storageConnectionString));
            services.AddSingleton<IQueueService, QueueService>();

            return services;
        }

    }
}