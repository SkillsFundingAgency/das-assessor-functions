using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Renci.SshNet;
using SFA.DAS.Assessor.Functions.ApplicationsMigrator;
using SFA.DAS.Assessor.Functions.Domain.Ilrs;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Ilrs.Services;
using SFA.DAS.Assessor.Functions.Domain.Print;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Assessor.Functions.Domain.Standards;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Configuration;
using SFA.DAS.Notifications.Api.Client.Configuration;
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
                var tempConfig = configBuilder
                    .Build();

                var configuration = configBuilder
                    .AddAzureTableStorageConfiguration(
                        tempConfig["ConfigurationStorageConnectionString"],
                        "SFA.DAS.AssessorFunctions",
                        tempConfig["EnvironmentName"],
                        "1.0")
                    .Build();

                return configuration;
            });

            var config = builder.GetCurrentConfiguration();

            builder.Services.AddOptions();
            
            builder.Services.Configure<AssessorApiAuthentication>(config.GetSection("AssessorApiAuthentication"));
            builder.Services.Configure<DataCollectionApiAuthentication>(config.GetSection("DataCollectionApiAuthentication"));
            builder.Services.Configure<RefreshIlrsSettings>(config.GetSection("RefreshIlrs"));
            builder.Services.Configure<SqlConnectionStrings>(config.GetSection("SqlConnectionStrings"));
            builder.Services.Configure<NotificationsApiClientConfiguration>(config.GetSection("NotificationsApiClientConfiguration"));
            builder.Services.Configure<CertificateDetails>(config.GetSection("CertificateDetails"));
            builder.Services.Configure<SftpSettings>(config.GetSection("SftpSettings"));

            builder.Services.AddSingleton<IAssessorServiceTokenService, AssessorServiceTokenService>();
            builder.Services.AddSingleton<IDataCollectionTokenService, DataCollectionTokenService>();

            builder.Services.AddScoped<AssessorTokenHandler>();
            builder.Services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>()
                .AddHttpMessageHandler<AssessorTokenHandler>();

            builder.Services.AddScoped<DataCollectionTokenHandler>();
            builder.Services.AddHttpClient<IDataCollectionServiceApiClient, DataCollectionServiceApiClient>()
                .AddHttpMessageHandler<DataCollectionTokenHandler>()
                .ConfigurePrimaryHttpMessageHandler(() => {
                    var handler = new HttpClientHandler();
                    if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    }
                    return handler;
                });

            builder.Services.AddScoped<IDateTimeHelper, DateTimeHelper>();

            builder.Services.AddScoped<IRefreshIlrsProviderService, RefreshIlrsProviderService>();
            builder.Services.AddScoped<IRefreshIlrsLearnerService, RefreshIlrsLearnerService>();
            
            builder.Services.AddTransient<IRefreshIlrsDequeueProvidersCommand, RefreshIlrsDequeueProvidersCommand>();
            builder.Services.AddTransient<IRefreshIlrsEnqueueProvidersCommand, RefreshIlrsEnqueueProvidersCommand>();

            builder.Services.AddTransient<IQnaDataTranslator, QnaDataTranslator>();
            builder.Services.AddTransient<IDataAccess, DataAccess>();

            if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
            {
                builder.Services.AddTransient<IFileTransferClient, NullFileTransferClient>();
            }
            else
            {
                builder.Services.AddTransient<IFileTransferClient, FileTransferClient>();
            }
            
            builder.Services.AddTransient<IPrintingJsonCreator, PrintingJsonCreator>();
            builder.Services.AddTransient<IPrintingSpreadsheetCreator, PrintingSpreadsheetCreator>();
            builder.Services.AddTransient<IPrintProcessCommand, PrintProcessCommand>();

            builder.Services.AddTransient<IStandardCollationImportCommand, StandardCollationImportCommand>();
            builder.Services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();

            builder.Services.AddTransient((s) =>
            {
                var sftpSettings = s.GetService<IOptions<SftpSettings>>()?.Value;
                return new SftpClient(sftpSettings.RemoteHost, Convert.ToInt32(sftpSettings.Port), sftpSettings.Username, sftpSettings.Password);
            });

            builder.Services.AddNotificationService();
        }
    }
}