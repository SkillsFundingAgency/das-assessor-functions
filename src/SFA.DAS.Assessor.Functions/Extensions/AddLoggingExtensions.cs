using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System.Reflection;

namespace SFA.DAS.Assessor.Functions.Extensions
{
    public static class AddLoggingExtensions
    {
        public static void AddLogging(this ILoggingBuilder logBuilder)
        {
            logBuilder.SetMinimumLevel(LogLevel.Trace);
            logBuilder.AddConsole();
            logBuilder.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });

            var nLogConfiguration = new NLogConfiguration();
            nLogConfiguration.ConfigureNLog();

        }
    }
}