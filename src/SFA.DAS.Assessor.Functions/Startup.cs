using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ApplicationsMigrator;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection.Authentication;
using SFA.DAS.Assessor.Functions.Infrastructure;
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

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureTableStorageConfiguration(
                    Environment.GetEnvironmentVariable("ConfigurationStorageConnectionString"),
                    Environment.GetEnvironmentVariable("AppName"),
                    Environment.GetEnvironmentVariable("EnvironmentName"),
                    "1.0", "SFA.DAS.AssessorFunctions")
                .Build();

            builder.Services.AddOptions();
            
            builder.Services.Configure<AssessorApiAuthentication>(config.GetSection("AssessorApiAuthentication"));
            builder.Services.Configure<DataCollectionApiAuthentication>(config.GetSection("DataCollectionApiAuthentication"));
            builder.Services.Configure<EpaoDataSync>(config.GetSection("EpaoDataSync"));
            builder.Services.Configure<SqlConnectionStrings>(config.GetSection("SqlConnectionStrings"));

            builder.Services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>();
            
            builder.Services.AddHttpClient<IDataCollectionServiceApiClient, DataCollectionServiceApiClient>(client => { })
                .ConfigurePrimaryHttpMessageHandler(() => {
                    var handler = new HttpClientHandler();
                    if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    }
                    return handler;
                });

            builder.Services.AddHttpClient<IDataCollectionServiceAnonymousApiClient, DataCollectionServiceAnonymousApiClient>(client => {})
                .ConfigurePrimaryHttpMessageHandler(() => {
                    var handler = new HttpClientHandler();
                    if (string.Equals("LOCAL", Environment.GetEnvironmentVariable("EnvironmentName")))
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    }
                    return handler;
                });

            builder.Services.AddScoped<IDataCollectionTokenService, DataCollectionTokenService>();
            builder.Services.AddScoped<IAssessorServiceTokenService, AssessorServiceTokenService>();

            builder.Services.AddScoped<IDateTimeHelper, DateTimeHelper>();
            builder.Services.AddScoped<IEpaoDataSyncProviderService, EpaoDataSyncProviderService>();
            builder.Services.AddScoped<IEpaoDataSyncLearnerService, EpaoDataSyncLearnerService>();
            
            builder.Services.AddTransient<IQnaDataTranslator, QnaDataTranslator>();
            builder.Services.AddTransient<IDataAccess, DataAccess>();
        }
    }
}