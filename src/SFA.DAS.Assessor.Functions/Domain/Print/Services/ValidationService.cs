using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class ValidationService : IDisposable
    {
        private readonly IFileTransferClient _fileTransferClient;
        private readonly string _fileName;
        private readonly string _filePath;
        private readonly string _content;
        private readonly List<ValidationDetails> _errors;

        public ValidationService(string fileName, string content, string filePath, IFileTransferClient fileTransferClient)
        {
            _fileTransferClient = fileTransferClient;
            _fileName = fileName;
            _filePath = filePath;
            _content = content;
            _errors = new List<ValidationDetails>();
        }

        public void Log(string field, string message)
        {
            _errors.Add(new ValidationDetails
            {
                Field = field,
                Message = message
            });
        }

        public void Dispose()
        {
            if (!_errors.Any()) return;

            using (var memoryStream = new MemoryStream())
            using (var sw = new StreamWriter(memoryStream))
            {
                sw.WriteLine("Begin File Content");
                sw.WriteLine(_content);
                sw.WriteLine("End File Content");
                sw.WriteLine();

                foreach (var error in _errors)
                {
                    sw.WriteLine("Error");
                    sw.WriteLine($"Field: {error.Field}");
                    sw.WriteLine($"Message: {error.Message}");
                    sw.WriteLine();
                }

                _fileTransferClient.Send(memoryStream, $"{_filePath}/{_fileName}.error");
            }
        }
    }
}
