using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.Assessors;
using SFA.DAS.Assessor.Functions.Domain.Assessors.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance;
using SFA.DAS.Assessor.Functions.Domain.DatabaseMaintenance.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync;
using SFA.DAS.Assessor.Functions.Domain.ExternalApiDataSync.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.FileTransfer;
using SFA.DAS.Assessor.Functions.Domain.Ilrs;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.Domain.Learners;
using SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ofs;
using SFA.DAS.Assessor.Functions.Domain.Print;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Assessor.Functions.Domain.Providers.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Standards;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Assessments;
using SFA.DAS.Assessor.Functions.Domain.Assessments.Interfaces;
using SFA.DAS.Assessor.Functions.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi;
using SFA.DAS.Assessor.Functions.ExternalApis.Approvals.OuterApi.Config;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs;
using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Config;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Configuration;
using SFA.DAS.Assessor.Functions.Infrastructure.Options;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.DatabaseMaintenance;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.ExternalApiDataSync;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.Learners;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.OfqualImport;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.PrintCertificates;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.Providers;
using SFA.DAS.Assessor.Functions.Infrastructure.Options.RefreshIlrs;
using SFA.DAS.Assessor.Functions.MockApis.DataCollection;
using SFA.DAS.Assessor.Functions.StartupConfiguration;
using SFA.DAS.AssessorService.Functions.Data;
using SFA.DAS.Http.TokenGenerators;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SFA.DAS.Assessor.Functions.StartupConfiguration
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddConfiguration((configBuilder) =>
                     {
                         var configuration = configBuilder
                             .AddConfiguration(builder.GetCurrentConfiguration())
                             .AddAzureTableStorageConfiguration(
                                 Environment.GetEnvironmentVariable("ConfigurationStorageConnectionString"),
                                 "SFA.DAS.AssessorFunctions",
                                 Environment.GetEnvironmentVariable("EnvironmentName"),
                                 "1.0")
                             .Build();

                         return configuration;
                     });

            var config = builder.GetCurrentConfiguration();

            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            });

            builder.Services.AddLogging(options =>
            {
                options.SetMinimumLevel(LogLevel.Information);

                options.AddFilter("Microsoft", LogLevel.Warning);
                options.AddFilter("System", LogLevel.Warning);
                options.AddFilter("SFA.DAS.Assessor.Functions", LogLevel.Information);
            });

            builder.Services.AddOptions();

            builder.Services.Configure<AssessorManagedIdentityClientConfiguration>(config.GetSection(nameof(AssessorManagedIdentityClientConfiguration)));
            builder.Services.Configure<DataCollectionApiAuthentication>(config.GetSection(nameof(DataCollectionApiAuthentication)));
            builder.Services.Configure<OfsRegisterApiAuthentication>(config.GetSection(nameof(OfsRegisterApiAuthentication)));
            builder.Services.Configure<SecureMessageApiAuthentication>(config.GetSection(nameof(SecureMessageApiAuthentication)));
            builder.Services.Configure<DataCollectionMock>(config.GetSection(nameof(DataCollectionMock)));

            var functionsOptions = nameof(FunctionsOptions);
            builder.Services.Configure<FunctionsOptions>(config.GetSection($"{functionsOptions}"));
            builder.Services.Configure<RebuildExternalApiSandboxOptions>(config.GetSection($"{functionsOptions}:{nameof(RebuildExternalApiSandboxOptions)}"));
            builder.Services.Configure<RefreshIlrsOptions>(config.GetSection($"{functionsOptions}:{nameof(RefreshIlrsOptions)}"));

            var databaseMaintenanceOptions = $"{functionsOptions}:{nameof(DatabaseMaintenanceOptions)}";
            builder.Services.Configure<DatabaseMaintenanceOptions>(config.GetSection(databaseMaintenanceOptions));

            var ofqualImportOptions = $"{functionsOptions}:{nameof(OfqualImportOptions)}";
            builder.Services.Configure<OfqualImportOptions>(config.GetSection(ofqualImportOptions));

            var printCertificatesOptions = $"{functionsOptions}:{nameof(PrintCertificatesOptions)}";
            builder.Services.Configure<PrintRequestOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(PrintRequestOptions)}"));
            builder.Services.Configure<CertificateDetails>(config.GetSection($"{printCertificatesOptions}:{nameof(PrintRequestOptions)}:{nameof(CertificateDetails)}"));
            builder.Services.Configure<PrintResponseOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(PrintResponseOptions)}"));
            builder.Services.Configure<DeliveryNotificationOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(DeliveryNotificationOptions)}"));
            builder.Services.Configure<BlobSasTokenGeneratorOptions>(config.GetSection($"{printCertificatesOptions}:{nameof(BlobSasTokenGeneratorOptions)}"));

            var importLearnerOptions = $"{functionsOptions}:{nameof(ImportLearnersOptions)}";
            builder.Services.Configure<ImportLearnersOptions>(config.GetSection(importLearnerOptions));

            var refreshProvidersOptions = $"{functionsOptions}:{nameof(RefreshProvidersOptions)}";
            builder.Services.Configure<RefreshProvidersOptions>(config.GetSection(refreshProvidersOptions));

            builder.Services.AddSingleton(s => new AssessorServiceTokenService(
              new ManagedIdentityTokenGenerator(
                  s.GetRequiredService<IOptions<AssessorManagedIdentityClientConfiguration>>().Value
                  ),
              s.GetRequiredService<ILogger<AssessorServiceTokenService>>()
              ) as IAssessorServiceTokenService);

            builder.Services.AddSingleton<IDataCollectionTokenService, DataCollectionTokenService>();
            builder.Services.AddSingleton<ISecureMessageTokenService, SecureMessageTokenService>();
            builder.Services.AddScoped<AssessorTokenHandler>();
            builder.Services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>()
                .AddHttpMessageHandler<AssessorTokenHandler>();

            builder.Services.AddScoped<DataCollectionTokenHandler>();

            var dataCollectionMock = config.GetSection(nameof(DataCollectionMock)).Get<DataCollectionMock>();
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
            if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
            {
                builder.Services.AddHttpClient<ISecureMessageServiceApiClient, SecureMessageServiceApiClientStub>();
            }
            else
            {
                builder.Services.AddHttpClient<ISecureMessageServiceApiClient, SecureMessageServiceApiClient>()
                    .AddHttpMessageHandler<SecureMessageTokenHandler>();
            }

            builder.Services.AddScoped<IDateTimeHelper, DateTimeHelper>();

            builder.Services.AddScoped<IRefreshIlrsAccessorSettingService, RefreshIlrsAccessorSettingService>();
            builder.Services.AddScoped<IRefreshIlrsAcademicYearService, RefreshIlrsAcademicYearService>();
            builder.Services.AddScoped<IRefreshIlrsProviderService, RefreshIlrsProviderService>();
            builder.Services.AddScoped<IRefreshIlrsLearnerService, RefreshIlrsLearnerService>();

            builder.Services.AddScoped<IBatchService, BatchService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            var storageConnectionString = config.GetValue<string>("AzureWebJobsStorage");
            var optionsCertificateFunctions = config.GetSection(functionsOptions).GetSection(nameof(printCertificatesOptions)).Get<PrintCertificatesOptions>();
            var optionsOfqualImport = config.GetSection(functionsOptions).GetSection(nameof(ofqualImportOptions)).Get<OfqualImportOptions>();

            builder.Services.AddTransient(s => new BlobFileTransferClient(
                s.GetRequiredService<ILogger<BlobFileTransferClient>>(),
                storageConnectionString,
                optionsCertificateFunctions.ExternalBlobContainer) as IExternalBlobFileTransferClient);

            builder.Services.AddTransient(s => new BlobFileTransferClient(
                s.GetRequiredService<ILogger<BlobFileTransferClient>>(),
                storageConnectionString,
                optionsCertificateFunctions.InternalBlobContainer) as IInternalBlobFileTransferClient);

            builder.Services.AddTransient(s => new BlobFileTransferClient(
                s.GetRequiredService<ILogger<BlobFileTransferClient>>(),
                storageConnectionString,
                optionsOfqualImport.DownloadBlobContainer) as IOfqualDownloadsBlobFileTransferClient);

            builder.Services.AddTransient<IPrintCreator, PrintingJsonCreator>();

            var optionsFunctions = builder.Services.BuildServiceProvider().GetService<IOptions<FunctionsOptions>>();
            builder.Services.AddDatabaseRegistration(optionsFunctions);
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IAssessorServiceRepository, AssessorServiceRepository>();

            builder.Services.AddTransient<IDatabaseMaintenanceCommand, DatabaseMaintenanceCommand>();
            builder.Services.AddTransient<IPrintRequestCommand, PrintRequestCommand>();
            builder.Services.AddTransient<IPrintResponseCommand, PrintResponseCommand>();
            builder.Services.AddTransient<IDeliveryNotificationCommand, DeliveryNotificationCommand>();
            builder.Services.AddTransient<IPrintStatusUpdateCommand, PrintStatusUpdateCommand>();
            builder.Services.AddTransient<IBlobStorageSamplesCommand, BlobStorageSamplesCommand>();
            builder.Services.AddTransient<IBlobSasTokenGeneratorCommand, BlobSasTokenGeneratorCommand>();
            builder.Services.AddTransient<IRebuildExternalApiSandboxCommand, RebuildExternalApiSandboxCommand>();
            builder.Services.AddTransient<IRefreshIlrsDequeueProvidersCommand, RefreshIlrsDequeueProvidersCommand>();
            builder.Services.AddTransient<IRefreshIlrsEnqueueProvidersCommand, RefreshIlrsEnqueueProvidersCommand>();
            builder.Services.AddTransient<IStandardImportCommand, StandardImportCommand>();
            builder.Services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();
            builder.Services.AddTransient<IAssessmentsSummaryUpdateCommand, AssessmentsSummaryUpdateCommand>();
            builder.Services.AddTransient<IImportLearnersCommand, ImportLearnersCommand>();
            builder.Services.AddTransient<IRefreshProvidersCommand, RefreshProvidersCommand>();
            builder.Services.AddTransient<IOfsImportCommand, OfsImportCommand>();

            builder.Services.Configure<OuterApi>(config.GetSection(nameof(OuterApi)));

            builder.Services.AddHttpClient<IOuterApiClient, OuterApiClient>();
            builder.Services.AddHttpClient<IOfsRegisterApiClient, OfsRegisterApiClient>();
            builder.Services.AddHttpClient(OfqualDataType.Organisations.ToString(), c => c.BaseAddress = new Uri(optionsOfqualImport.OrganisationsDataUrl));
            builder.Services.AddHttpClient(OfqualDataType.Qualifications.ToString(), c => c.BaseAddress = new Uri(optionsOfqualImport.QualificationsDataUrl));
            builder.Services.AddTransient<IEnqueueLearnerInfoCommand, EnqueueLearnerInfoCommand>();
            builder.Services.AddTransient<IDequeueLearnerInfoCommand, DequeueLearnerInfoCommand>();
            builder.Services.AddTransient<IEnqueueApprovalLearnerInfoBatchCommand, EnqueueApprovalLearnerInfoBatchCommand>();
        }
    }
}