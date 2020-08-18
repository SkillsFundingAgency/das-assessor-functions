using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Renci.SshNet;
using SFA.DAS.Assessor.Functions.Domain.Print;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Assessor.Functions.Domain.Standards;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.ExternalApis;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;

[assembly: FunctionsStartup(typeof(SFA.DAS.Assessor.Functions.Startup))]

namespace SFA.DAS.Assessor.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.BuildServiceProvider();

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
            builder.Services.Configure<SqlConnectionStrings>(config.GetSection("SqlConnectionStrings"));
            builder.Services.Configure<NotificationsApiClientConfiguration>(config.GetSection("NotificationsApiClientConfiguration"));
            builder.Services.Configure<CertificateDetails>(config.GetSection("CertificateDetails"));
            builder.Services.Configure<SftpSettings>(config.GetSection("SftpSettings"));

            builder.Services.AddSingleton<IAssessorServiceTokenService, AssessorServiceTokenService>();
            
            builder.Services.AddScoped<AssessorTokenHandler>();
            builder.Services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>()
                .AddHttpMessageHandler<AssessorTokenHandler>();

            builder.Services.AddScoped<IBatchService, BatchService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();

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
            builder.Services.AddTransient<IDeliveryNotificationCommand, DeliveryNotificationCommand>();
            builder.Services.AddTransient<IPrintNotificationCommand, PrintNotificationCommand>();

            builder.Services.AddTransient<IStandardCollationImportCommand, StandardCollationImportCommand>();
            builder.Services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();

            builder.Services.AddTransient((s) =>
            {
                var sftpSettings = s.GetService<IOptions<SftpSettings>>()?.Value;
                return new SftpClient(sftpSettings.RemoteHost, Convert.ToInt32(sftpSettings.Port), sftpSettings.Username, sftpSettings.Password);
            });            
        }
    }
}