using System;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Exceptions
{
    public class PrintRequestException : Exception
    {
        public PrintRequestException(string message)
            : base(message)
        {
        }

        public PrintRequestException(string message, Exception innerException)
            : base(message, innerException)
        { 
        }
    }
}
