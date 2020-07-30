using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Exceptions
{
    public class FileFormatValidationException : Exception
    {
        public FileFormatValidationException()
        {
        }

        public FileFormatValidationException(string message) : base(message)
        {
        }

        public FileFormatValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
