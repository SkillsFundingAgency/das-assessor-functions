using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Assessor.Functions.Logger
{
    public class AggregateLogger : IAggregateLogger
    {
        private readonly ILogger _functionLogger;
        private readonly global::NLog.Logger _redisLogger;
        private readonly string _source;

        public AggregateLogger(string source, ILogger functionLogger, ExecutionContext executionContext)
        {
            _source = source;
            _functionLogger = functionLogger;

            var nLogFileName = GetNLogConfigurationFileName(source);

            LogManager.Configuration = new XmlLoggingConfiguration($@"{executionContext.FunctionAppDirectory}\{nLogFileName}.config");
            _redisLogger = LogManager.GetCurrentClassLogger();
        }

        public void LogError(Exception ex, string message)
        {
            _functionLogger.LogError(ex, message, _source);
            _redisLogger.Error(ex, message);
        }

        public void LogInformation(string message)
        {
            _functionLogger.LogInformation(message, _source);
            _redisLogger.Info(message);
        }

        public void LogDebug(string message)
        {
            _functionLogger.LogDebug(message, _source);
            _redisLogger.Debug(message);
        }

        private string GetNLogConfigurationFileName(string source)
        {
            var nLogFileName = "nlog." + source.Split('-').Last().Trim();
            return nLogFileName;
        }
    }
}