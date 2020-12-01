using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Types
{
    public class PrintFileInfo
    {
        public string FileName { get; }
        public string FileContent { get; }
        public string InvalidFileContent { get; set; }
        public List<string> ValidationMessages { get; set; }

        public PrintFileInfo(string fileContent, string fileName)
        {
            FileContent = fileContent;
            FileName = fileName;
            ValidationMessages = new List<string>();
        }
    }
}
