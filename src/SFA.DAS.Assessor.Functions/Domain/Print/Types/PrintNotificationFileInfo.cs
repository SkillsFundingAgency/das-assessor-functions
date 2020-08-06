
namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class PrintNotificationFileInfo
    {
        public string FileName { get; }
        public string FileContent { get; }

        public PrintNotificationFileInfo(string fileContent, string fileName)
        {
            FileContent = fileContent;
            FileName = fileName;
        }
    }
}
