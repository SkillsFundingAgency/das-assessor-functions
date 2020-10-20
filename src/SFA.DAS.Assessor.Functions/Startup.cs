using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Print;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Services;
using SFA.DAS.Assessor.Functions.Domain.Standards;
using SFA.DAS.Assessor.Functions.Domain.Standards.Interfaces;
using SFA.DAS.Assessor.Functions.Extensions;
using SFA.DAS.Assessor.Functions.ExternalApis;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Authentication;
using SFA.DAS.Assessor.Functions.Infrastructure;
using SFA.DAS.Assessor.Functions.Infrastructure.Configuration;

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
            builder.Services.Configure<CertificateDetails>(config.GetSection("CertificateDetails"));
            builder.Services.Configure<CertificatePrintFunctionSettings>(config.GetSection("FunctionsSettings:CertificatePrintFunction"));
            builder.Services.Configure<CertificatePrintNotificationFunctionSettings>(config.GetSection("FunctionsSettings:CertificatePrintNotificationFunction"));
            builder.Services.Configure<CertificateDeliveryNotificationFunctionSettings>(config.GetSection("FunctionsSettings:CertificateDeliveryNotificationFunction"));

            builder.Services.AddSingleton<IAssessorServiceTokenService, AssessorServiceTokenService>();
            
            builder.Services.AddScoped<AssessorTokenHandler>();
            builder.Services.AddHttpClient<IAssessorServiceApiClient, AssessorServiceApiClient>()
                .AddHttpMessageHandler<AssessorTokenHandler>();

            builder.Services.AddScoped<IBatchService, BatchService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();

            var storageConnectionString = config.GetValue<string>("AzureWebJobsStorage");
            builder.Services.AddTransient<IFileTransferClient>(s =>
                new BlobTransferClient(s.GetRequiredService<ILogger<BlobTransferClient>>(), storageConnectionString));

            builder.Services.AddTransient<IPrintCreator, PrintingJsonCreator>();
            builder.Services.AddTransient<IPrintProcessCommand, PrintProcessCommand>();
            builder.Services.AddTransient<IDeliveryNotificationCommand, DeliveryNotificationCommand>();
            builder.Services.AddTransient<IPrintNotificationCommand, PrintNotificationCommand>();
            builder.Services.AddTransient<IStandardCollationImportCommand, StandardCollationImportCommand>();
            builder.Services.AddTransient<IStandardSummaryUpdateCommand, StandardSummaryUpdateCommand>();
            
            builder.Services.AddTransient<INotificationService, NotificationService>();
        }
    }
}