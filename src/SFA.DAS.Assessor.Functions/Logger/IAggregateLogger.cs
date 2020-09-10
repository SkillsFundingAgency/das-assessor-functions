using System;

namespace SFA.DAS.Assessor.Functions.Logger
{
    public interface IAggregateLogger
    {
        void LogError(Exception ex, string message);
        void LogInformation(string message);
        void LogDebug(string message);
    }
}
