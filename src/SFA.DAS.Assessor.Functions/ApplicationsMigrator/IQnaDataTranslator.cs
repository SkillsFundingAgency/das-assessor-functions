namespace SFA.DAS.Assessor.Functions.ApplicationsMigrator
{
    public interface IQnaDataTranslator
    {
        string Translate(dynamic applicationSection, dynamic applySequence, Microsoft.Extensions.Logging.ILogger log);
    }
}