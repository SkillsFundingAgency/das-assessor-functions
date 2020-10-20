namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IValidationService
    {
        void Start(string fileName, string content, string filePath);
        void Log(string field, string message);
        void End();
    }
}
