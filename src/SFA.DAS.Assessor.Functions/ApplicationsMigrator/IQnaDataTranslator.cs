namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public interface IQnaDataTranslator
    {
        string Translate(dynamic applicationSection, Microsoft.Extensions.Logging.ILogger log);
    }
}